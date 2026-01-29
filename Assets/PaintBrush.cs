using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PaintBrush : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Potter pottery;
    [SerializeField] private LayerMask potLayerMask = ~0;

    [Header("Brush Settings")]
    [SerializeField] private float brushRadiusUV = 0.02f;
    [SerializeField] private Color brushColor = Color.red;
    [SerializeField] private bool erase = false;
    [SerializeField] private float raycastDistance = 0.2f;

    [Header("Ray Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Input")]
    [SerializeField] private InputActionProperty paintAction;

    private Collider col;
    private bool canPaint;

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;

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
    }
    
    public void SetBrushColor(Color newColor) {
        brushColor = newColor;
    }

    public Color GetBrushColor() {
        return brushColor;
    }


    private void OnTriggerStay(Collider other)
    {
        if (!canPaint) return;
        if (!pottery) return;


        Vector3 origin = rayOrigin.position;
        Vector3 dir = rayOrigin.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, raycastDistance, potLayerMask, triggerInteraction))
        {
            if (hit.collider.GetComponentInParent<Potter>() == pottery)
            {
                PaintAtUV(hit.textureCoord);
            }
        }
    }

    private void PaintAtUV(Vector2 uv)
    {
        Texture2D tex = pottery.GetPaintTexture();
        if (tex == null) return;

        int w = tex.width;
        int h = tex.height;

        float u = uv.x;
        float v = uv.y;
        if (u >= 1f) u = 1f - Mathf.Epsilon;

        int centerX = Mathf.RoundToInt(u * w);
        int centerY = Mathf.RoundToInt(v * h);

        int radiusPixels = Mathf.Max(1, Mathf.RoundToInt(brushRadiusUV * w));
        Color target = erase ? Color.clear : brushColor;

        DrawDisc(tex, centerX, centerY, radiusPixels, target);

        if (centerX - radiusPixels < 0)
            DrawDisc(tex, centerX + w, centerY, radiusPixels, target);

        if (centerX + radiusPixels >= w)
            DrawDisc(tex, centerX - w, centerY, radiusPixels, target);

        tex.Apply();
    }

    private static void DrawDisc(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int w = tex.width;
        int h = tex.height;

        int xMin = Mathf.Clamp(cx - r, 0, w - 1);
        int xMax = Mathf.Clamp(cx + r, 0, w - 1);
        int yMin = Mathf.Clamp(cy - r, 0, h - 1);
        int yMax = Mathf.Clamp(cy + r, 0, h - 1);

        int r2 = r * r;

        for (int y = yMin; y <= yMax; y++)
        {
            int dy = y - cy;
            for (int x = xMin; x <= xMax; x++)
            {
                int dx = x - cx;
                if (dx * dx + dy * dy <= r2)
                    tex.SetPixel(x, y, col);
            }
        }
    }
}