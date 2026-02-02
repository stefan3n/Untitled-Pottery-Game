using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public sealed class Potter : MonoBehaviour
{
    public int faces = 16;

    [Header("Vertical layout")]
    public float baseRingHeight = 0.2f;
    public float baseRingRadius = 0.3f;
    public int ringsCount = 3;

    public float maxPotHeight = 1.2f;

    [HideInInspector]
    public float[] ringsRadius;

    [HideInInspector]
    public float[] ringHeights;

    [Header("State")]
    public bool isStatic = false;

    private float[] defaultRadius;
    private float[] defaultRingHeights;

    [Header("UV options")]
    public bool flipUInside = false;

    [Header("Radius Limits")]
    public float minRingRadius = 0.15f;
    public float maxRingRadius = 0.9f;

    [Header("Painting")]
    [SerializeField] private Material potPaintMaterial;
    [SerializeField] private int paintTextureSize = 1024;
    [SerializeField] private string paintTextureProperty = "_PaintTex";

    private Texture2D paintTexture;
    private MeshRenderer meshRenderer;

    private bool isModified = false;

    Mesh mesh;
    Body body;
    MeshCollider meshCollider;

    void Awake()
    {
        if (baseRingHeight <= 0.0f || baseRingRadius <= 0.0f || ringsCount <= 2 || maxPotHeight < baseRingHeight * (ringsCount - 1))
        {
            Debug.LogError("Invalid values for Pot parameters!");
            return;
        }

        bool needReinit = false;
        if (ringsRadius == null || ringsRadius.Length != ringsCount) needReinit = true;
        if (ringHeights == null || ringHeights.Length != ringsCount) needReinit = true;

        if (needReinit)
        {
            ringsRadius = new float[ringsCount];
            ringHeights = new float[ringsCount];
            for (int i = 0; i < ringsCount; i++)
            {
                ringsRadius[i] = baseRingRadius;
                ringHeights[i] = baseRingHeight * i;
            }
        }

        defaultRadius = (float[])ringsRadius.Clone();
        defaultRingHeights = (float[])ringHeights.Clone();

        body = new Body(faces, ringsCount, ringHeights, ringsRadius);

        mesh = new Mesh { name = "Pot" };
        mesh.MarkDynamic();

        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();

        SetupMaterials();
        GenerateMesh();
    }

    private void SetupMaterials()
    {
        if (!potPaintMaterial)
        {
            Debug.LogError("Potter: potPaintMaterial is not assigned!");
            return;
        }

        meshRenderer.materials = new[] { potPaintMaterial, potPaintMaterial };

        paintTexture = new Texture2D(paintTextureSize, paintTextureSize, TextureFormat.RGBA32, false);
        ClearPaintTexture(Color.clear);

        var mats = meshRenderer.materials;
        foreach (var mat in mats)
        {
            if (mat != null && mat.HasProperty(paintTextureProperty))
            {
                mat.SetTexture(paintTextureProperty, paintTexture);
            }
        }
        meshRenderer.materials = mats;
    }

    public Texture2D GetPaintTexture()
    {
        return paintTexture;
    }

    public void MarkModified()
    {
        isModified = true;
    }

    public void ClearPaintTexture(Color c)
    {
        if (paintTexture == null) return;

        Color[] cols = new Color[paintTexture.width * paintTexture.height];
        for (int i = 0; i < cols.Length; i++)
            cols[i] = c;

        paintTexture.SetPixels(cols);
        paintTexture.Apply();
    }

    void Update()
    {
        if (isStatic || !isModified) return;

        for (int i = 0; i < ringsRadius.Length; i++)
        {
            ringsRadius[i] = Mathf.Clamp(ringsRadius[i], minRingRadius, maxRingRadius);
        }

        GenerateMesh();
        isModified = false;
    }

    public void GenerateMesh()
    {
        if (body == null) return;

        if (body.vertices.GetLength(1) != ringsCount)
        {
            body = new Body(faces, ringsCount, ringHeights, ringsRadius);
        }

        body.InitializeVertices();

        int facesN = body.vertices.GetLength(0);
        int ringsN = body.vertices.GetLength(1);
        int vCount = facesN * ringsN;

        Vector3[] posOut = body.VerticesToPositionArray();
        Vector3[] nrmOut = body.VerticesToNormalsArray();

        Vector3[] vertices = new Vector3[vCount * 2];
        Vector3[] normals = new Vector3[vCount * 2];
        Vector2[] uvs = new Vector2[vCount * 2];

        Vector2[] uvOut = new Vector2[vCount];
        for (int y = 0; y < ringsN; y++)
        {
            for (int x = 0; x < facesN; x++)
            {
                int i = x + y * facesN;
                float u = facesN > 1 ? x / (facesN - 1f) : 0f;

                float totalH = GetTotalHeight();
                float currentH = ringHeights[y] - ringHeights[0];
                float v = (totalH > 0) ? currentH / totalH : 0f;

                uvOut[i] = new Vector2(u, v);
            }
        }

        // Outside
        for (int i = 0; i < vCount; i++)
        {
            vertices[i] = posOut[i];
            normals[i] = nrmOut[i];
            uvs[i] = uvOut[i];
        }

        // Inside
        int offset = vCount;
        for (int i = 0; i < vCount; i++)
        {
            vertices[offset + i] = posOut[i];
            normals[offset + i] = -nrmOut[i];

            if (flipUInside)
                uvs[offset + i] = new Vector2(1f - uvOut[i].x, uvOut[i].y);
            else
                uvs[offset + i] = uvOut[i];
        }

        mesh.Clear();
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;

        int triPerQuad = 6;
        int quadCount = (facesN - 1) * (ringsN - 1);
        int triCount = quadCount * triPerQuad;

        int[] outsideTris = new int[triCount];
        int t = 0;
        for (int y = 0; y < ringsN - 1; y++)
        {
            for (int x = 0; x < facesN - 1; x++)
            {
                int a = body.vertices[x, y].index;
                int b = body.vertices[x, y + 1].index;
                int c = body.vertices[x + 1, y + 1].index;
                int d = body.vertices[x + 1, y].index;

                outsideTris[t++] = a; outsideTris[t++] = b; outsideTris[t++] = c;
                outsideTris[t++] = a; outsideTris[t++] = c; outsideTris[t++] = d;
            }
        }

        int[] insideTris = new int[triCount];
        t = 0;
        for (int y = 0; y < ringsN - 1; y++)
        {
            for (int x = 0; x < facesN - 1; x++)
            {
                int a = body.vertices[x, y].index + offset;
                int b = body.vertices[x, y + 1].index + offset;
                int c = body.vertices[x + 1, y + 1].index + offset;
                int d = body.vertices[x + 1, y].index + offset;

                insideTris[t++] = c; insideTris[t++] = b; insideTris[t++] = a;
                insideTris[t++] = d; insideTris[t++] = c; insideTris[t++] = a;
            }
        }

        mesh.subMeshCount = 2;
        mesh.SetTriangles(outsideTris, 0);
        mesh.SetTriangles(insideTris, 1);

        mesh.RecalculateBounds();

        if (meshCollider)
        {
            meshCollider.enabled = false;
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            meshCollider.enabled = true;
        }
    }

    
    public float[] GetRadiiData()
    {
        return (float[])ringsRadius.Clone();
    }

    public float[] GetHeightsData()
    {
        return (float[])ringHeights.Clone();
    }

    public void LoadPotData(float[] newRadii, float[] newHeights)
    {
        if (newRadii == null || newHeights == null || newRadii.Length != newHeights.Length) return;

        for (int i = 0; i < newRadii.Length; i++)
        {
            if (newRadii[i] < 0.01f) newRadii[i] = 0.05f;
        }

        ringsCount = newRadii.Length;
        ringsRadius = (float[])newRadii.Clone();
        ringHeights = (float[])newHeights.Clone();

        body = new Body(faces, ringsCount, ringHeights, ringsRadius);

        GenerateMesh();
    }

    public void ResetPot()
    {
        if (defaultRadius != null && defaultRingHeights != null)
        {
            LoadPotData(defaultRadius, defaultRingHeights);
        }
    }

    public float GetTotalHeight()
    {
        if (ringHeights == null || ringHeights.Length == 0)
            return 0f;

        return ringHeights[ringHeights.Length - 1] - ringHeights[0];
    }

    public void InsertRingBetween(int lowerIndex, int upperIndex)
    {
        if (lowerIndex < 0 || upperIndex >= ringsCount || upperIndex != lowerIndex + 1) return;

        int newCount = ringsCount + 1;

        float newHeight = 0.5f * (ringHeights[lowerIndex] + ringHeights[upperIndex]);
        float newRadius = 0.5f * (ringsRadius[lowerIndex] + ringsRadius[upperIndex]);

        var newRadii = new float[newCount];
        var newHeights = new float[newCount];

        int write = 0;
        for (int i = 0; i < ringsCount; i++)
        {
            newRadii[write] = ringsRadius[i];
            newHeights[write] = ringHeights[i];
            write++;

            if (i == lowerIndex)
            {
                newRadii[write] = newRadius;
                newHeights[write] = newHeight;
                write++;
            }
        }

        ringsCount = newCount;
        ringsRadius = newRadii;
        ringHeights = newHeights;

        GenerateMesh();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (mesh == null || body == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawWireMesh(mesh);
    }
#endif
}