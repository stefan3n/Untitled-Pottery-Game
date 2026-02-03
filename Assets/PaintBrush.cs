using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PaintBrush : MonoBehaviour
{
    public enum BrushMode
    {
        Normal,
        Erase,
        Spray,
        Bucket
    }

    [Header("References")]
    [SerializeField] private Potter pottery;
    [SerializeField] private LayerMask potLayerMask = ~0;
    [SerializeField] private ParticleSystem paintParticles;

    [Header("Brush Settings")]
    [SerializeField] private BrushMode currentMode = BrushMode.Normal;
    [SerializeField] private Color brushColor = Color.red;
    [SerializeField, Range(0.001f, 0.2f)] private float brushRadiusUV = 0.01f;
    [SerializeField, Range(0.01f, 1f)] private float sprayDensity = 0.1f;
    [SerializeField] private float raycastDistance = 0.2f;

    [Header("UI Control Limits")]
    [SerializeField] private float minBrushSize = 0.01f;
    [SerializeField] private float maxBrushSize = 0.1f;

    [Header("Ray Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Input")]
    [SerializeField] private InputActionProperty paintAction;
    
    [Header("Shared Material")] 
    public Material targetMaterial;

    private Collider col;
    private bool canPaint;

    private float lastBucketPaintTime = 0f;
    private const float BUCKET_COOLDOWN = 0.2f;

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;

        if (paintParticles)
        {
            var main = paintParticles.main;
            main.startColor = brushColor;
        }

        if (targetMaterial)
        {
            targetMaterial.color = brushColor; 
        }
        
        if (rayOrigin == null) rayOrigin = transform;
    }

    private void OnEnable()
    {
        if (paintAction.action != null)
            paintAction.action.Enable();
    }

    private void OnDisable()
    {
        if (paintAction.action != null)
            paintAction.action.Disable();
    }

    private void Update()
    {
        float val = paintAction.action?.ReadValue<float>() ?? 0f;
        canPaint = val > 0.5f;
        
        if (paintParticles)
        {
            if (canPaint && !paintParticles.isPlaying)
            {
                paintParticles.Play();
            }
            else if (!canPaint && paintParticles.isPlaying)
            {
                paintParticles.Stop();
            }
        }
    }

    public void SetBrushColor(Color newColor)
    {
        brushColor = newColor;
        if (paintParticles)
        {
            var main = paintParticles.main;
            main.startColor = newColor;
        }
        
        if (targetMaterial)
        {
            targetMaterial.color = newColor; 
        }
    }

    public Color GetBrushColor()
    {
        return brushColor;
    }

    public void SetBrushSize(float sliderValue01)
    {
        brushRadiusUV = Mathf.Lerp(minBrushSize, maxBrushSize, Mathf.Clamp01(sliderValue01));
    }

    public void SetBrushModeNormal() => currentMode = BrushMode.Normal;
    public void SetBrushModeErase() => currentMode = BrushMode.Erase;
    public void SetBrushModeSpray() => currentMode = BrushMode.Spray;
    public void SetBrushModeBucket() => currentMode = BrushMode.Bucket;

    public void SetBrushMode(int modeIndex)
    {
        if (modeIndex >= 0 && modeIndex < System.Enum.GetValues(typeof(BrushMode)).Length)
            currentMode = (BrushMode)modeIndex;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!canPaint) return;
        if (!pottery) return;

        Vector3 origin = rayOrigin.position;
        Vector3 dir = rayOrigin.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, raycastDistance, potLayerMask, triggerInteraction))
        {
            var hitPotter = hit.collider.GetComponentInParent<Potter>();
            if (hitPotter != pottery) return;

            if (currentMode == BrushMode.Bucket)
            {
                if (Time.time - lastBucketPaintTime > BUCKET_COOLDOWN)
                {
                    pottery.ClearPaintTexture(brushColor);
                    lastBucketPaintTime = Time.time;
                }
                return;
            }

            int submeshIndex = 0;
            if (hit.collider is MeshCollider && pottery.ringsCount > 0 && pottery.faces > 0)
            {
                int numQuads = (pottery.faces - 1) * (pottery.ringsCount - 1);
                int trisPerSubmesh = numQuads * 2;
                if (hit.triangleIndex >= trisPerSubmesh)
                {
                    submeshIndex = 1;
                }
            }

            Texture2D tex = pottery.GetPaintTexture(submeshIndex);
            if (tex == null) return;

            Vector2 pixelUV = hit.textureCoord;
            
            int texW = tex.width;
            int texH = tex.height;

            int centerX = (int)(pixelUV.x * texW);
            int centerY = (int)(pixelUV.y * texH);
            
            int radiusPixels = (int)(brushRadiusUV * texW);
            int r2 = radiusPixels * radiusPixels;

            bool modified = false;

            Color targetColor = (currentMode == BrushMode.Erase) ? Color.clear : brushColor;

            int xMin = Mathf.Clamp(centerX - radiusPixels, 0, texW - 1);
            int xMax = Mathf.Clamp(centerX + radiusPixels, 0, texW - 1);
            int yMin = Mathf.Clamp(centerY - radiusPixels, 0, texH - 1);
            int yMax = Mathf.Clamp(centerY + radiusPixels, 0, texH - 1);


            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;

                    if (dx * dx + dy * dy < r2)
                    {
                        if (currentMode == BrushMode.Spray)
                        {
                            if (Random.value > sprayDensity) continue;
                        }

                        tex.SetPixel(x, y, targetColor);
                        modified = true;
                    }
                }
            }
            
            if (centerX - radiusPixels < 0 || centerX + radiusPixels >= texW)
            {
                int offset = (centerX < texW / 2) ? texW : -texW;
                int wrappedCenterX = centerX + offset;
                
                int wxMin = Mathf.Clamp(wrappedCenterX - radiusPixels, 0, texW - 1);
                int wxMax = Mathf.Clamp(wrappedCenterX + radiusPixels, 0, texW - 1);

                for (int y = yMin; y <= yMax; y++)
                {
                    for (int x = wxMin; x <= wxMax; x++)
                    {
                        int dx = x - wrappedCenterX;
                        int dy = y - centerY;
                        if (dx * dx + dy * dy < r2)
                        {
                            if (currentMode == BrushMode.Spray && Random.value > sprayDensity) continue;
                            tex.SetPixel(x, y, targetColor);
                            modified = true;
                        }
                    }
                }
            }

            if (modified)
            {
                tex.Apply();
            }
        }
    }
}
