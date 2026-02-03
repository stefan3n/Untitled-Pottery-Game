using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public sealed class Potter : MonoBehaviour
{
    public int faces = 16;

    [Header("Vertical layout")]
    public float baseRingHeight = 0.05f;
    public float baseRingRadius = 0.3f;
    public int ringsCount = 12;

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
    public float minRingRadius = 0.05f;
    public float maxRingRadius = 0.8f;

    [Header("Painting")]
    [SerializeField] private Material potPaintMaterial;
    [SerializeField] private int paintTextureSize = 1024;
    [SerializeField] private string paintTextureProperty = "_PaintTex";

    private Texture2D paintTexture;      
    private Texture2D paintTextureInside;  
    
    private MeshRenderer meshRenderer;

    private bool isModified = false;

    Mesh mesh;
    Body body;
    MeshCollider meshCollider;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "PotMesh";

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        if (potPaintMaterial != null)
        {
            meshRenderer.materials = new[]
            {
                new Material(potPaintMaterial),
                new Material(potPaintMaterial)
            };
        }

        paintTexture = CreatePaintTexture();
        paintTextureInside = CreatePaintTexture();
        
        var mats = meshRenderer.materials;

        if (mats.Length > 0 && mats[0] != null && mats[0].HasProperty(paintTextureProperty))
        {
            mats[0].SetTexture(paintTextureProperty, paintTexture);
        }

        if (mats.Length > 1 && mats[1] != null && mats[1].HasProperty(paintTextureProperty))
        {
            mats[1].SetTexture(paintTextureProperty, paintTextureInside);
        }

        meshRenderer.materials = mats;
        
        meshCollider = GetComponent<MeshCollider>();

        if (ringsRadius == null || ringsRadius.Length != ringsCount)
        {
            ResetPot();
        }
    }

    private MeshFilter meshFilter;

    public Texture2D GetPaintTexture(int submeshIndex = 0)
    {
        if (submeshIndex == 1) return paintTextureInside;
        return paintTexture;
    }
    
    private Texture2D CreatePaintTexture()
    {
        var tex = new Texture2D(paintTextureSize, paintTextureSize, TextureFormat.RGBA32, false);
        
        var fill = Color.clear;
        var fills = new Color[paintTextureSize * paintTextureSize];
        for (int i = 0; i < fills.Length; i++)
            fills[i] = fill;
        
        tex.SetPixels(fills);
        tex.Apply();
        
        return tex;
    }

    public void MarkModified()
    {
        isModified = true;
    }

    public void SetPaintTexture()
    {
        float h = GetTotalHeight();
        float avgRadius = 0f;

        if (ringsRadius != null && ringsCount > 0)
        {
            for (int i = 0; i < ringsCount; i++)
            {
                avgRadius += ringsRadius[i];
            }
            avgRadius /= ringsCount;
        }

        float circumference = 2f * Mathf.PI * avgRadius;

        if (circumference > 0.0001f)
        {
            int newHeight = Mathf.RoundToInt(paintTextureSize * (h / circumference));
            newHeight = Mathf.Clamp(newHeight, 16, 8192);

            ResizeAndClear(paintTexture, newHeight);
            ResizeAndClear(paintTextureInside, newHeight);
        }
    }

    private void ResizeAndClear(Texture2D tex, int newHeight)
    {
        if (tex == null) return;
        if (tex.height != newHeight)
        {
            tex.Reinitialize(paintTextureSize, newHeight);
            ClearTexture(tex, Color.clear);
        }
    }

    private void ClearTexture(Texture2D tex, Color c)
    {
        if (tex == null) return;
        var cols = tex.GetPixels();
        for (int i = 0; i < cols.Length; ++i) cols[i] = c;
        tex.SetPixels(cols);
        tex.Apply();
    }

    public void ClearPaintTexture(Color c)
    {
        ClearTexture(paintTexture, c);
        ClearTexture(paintTextureInside, c);
    }

    void Update()
    {
        if (isStatic) return;

        if (isModified)
        {
            GenerateMesh();
            isModified = false;
        }
    }

    public void GenerateMesh()
    {
        if (body == null || body.vertices.GetLength(1) != ringsCount)
        {
            body = new Body(faces, ringsCount, ringHeights, ringsRadius);
        }

        body.InitializeVertices();

        Vector3[] vOut = body.VerticesToPositionArray();
        Vector3[] nOut = body.VerticesToNormalsArray();

        int vertexCountOneSide = vOut.Length;
        int totalVertices = vertexCountOneSide * 2;

        Vector3[] vertices = new Vector3[totalVertices];
        Vector3[] normals = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];

        float totalH = GetTotalHeight();
        
        Array.Copy(vOut, 0, vertices, 0, vertexCountOneSide);
        Array.Copy(nOut, 0, normals, 0, vertexCountOneSide);
        
        for (int i = 0; i < vertexCountOneSide; i++)
        {
            vertices[vertexCountOneSide + i] = vOut[i];
            normals[vertexCountOneSide + i] = -nOut[i];
        }

        for (int y = 0; y < ringsCount; y++)
        {
            float v = ringHeights[y] / totalH;

            for (int x = 0; x < faces; x++)
            {
                int indexOut = y * faces + x;
                int indexIn = vertexCountOneSide + indexOut;

                float u = (float)x / (faces - 1);
                
                uvs[indexOut] = new Vector2(u, v);

                float uIn = flipUInside ? (1f - u) : u;
                uvs[indexIn] = new Vector2(uIn, v);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;

        // Triangles
        mesh.subMeshCount = 2;

        int numQuads = (faces - 1) * (ringsCount - 1);
        int[] trisOut = new int[numQuads * 6];
        int[] trisIn = new int[numQuads * 6];

        int t = 0;
        for (int y = 0; y < ringsCount - 1; y++)
        {
            for (int x = 0; x < faces - 1; x++)
            {
                int bl = y * faces + x;
                int br = y * faces + (x + 1);
                int tl = (y + 1) * faces + x;
                int tr = (y + 1) * faces + (x + 1);

                // Outside (CCW)
                trisOut[t] = bl;
                trisOut[t + 1] = tl;
                trisOut[t + 2] = br;
                trisOut[t + 3] = br;
                trisOut[t + 4] = tl;
                trisOut[t + 5] = tr;
                
                int off = vertexCountOneSide;
                trisIn[t] = off + bl;
                trisIn[t + 1] = off + br;
                trisIn[t + 2] = off + tl;
                trisIn[t + 3] = off + br;
                trisIn[t + 4] = off + tr;
                trisIn[t + 5] = off + tl;

                t += 6;
            }
        }

        mesh.SetTriangles(trisOut, 0);
        mesh.SetTriangles(trisIn, 1);

        mesh.RecalculateBounds();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    public float[] GetRadiiData() {
        return (float[])ringsRadius.Clone();
    }

    public void SetRadiiData(float[] newData) {
        if (newData.Length != ringsCount) return;
        ringsRadius = newData;
        isModified = true;
    }

    public void ResetPot() {
        ringsRadius = new float[ringsCount];
        ringHeights = new float[ringsCount];
        defaultRadius = new float[ringsCount];
        defaultRingHeights = new float[ringsCount];

        for (int i = 0; i < ringsCount; i++)
        {
            ringsRadius[i] = baseRingRadius;
            ringHeights[i] = i * baseRingHeight;

            defaultRadius[i] = ringsRadius[i];
            defaultRingHeights[i] = ringHeights[i];
        }

        isModified = true;
    }

    public float GetTotalHeight()
    {
        if (ringHeights == null || ringHeights.Length == 0) return 0f;
        return ringHeights[ringsCount - 1] - ringHeights[0];
    }

    public void InsertRingBetween(int lowerIndex, int upperIndex)
    {
        if (lowerIndex < 0 || upperIndex >= ringsCount || (upperIndex - lowerIndex) != 1) return;

        int newCount = ringsCount + 1;
        float[] newR = new float[newCount];
        float[] newH = new float[newCount];
        
        float avgH = (ringHeights[lowerIndex] + ringHeights[upperIndex]) * 0.5f;
        float avgR = (ringsRadius[lowerIndex] + ringsRadius[upperIndex]) * 0.5f;

        for (int i = 0; i <= lowerIndex; i++) {
            newR[i] = ringsRadius[i];
            newH[i] = ringHeights[i];
        }

        newR[lowerIndex + 1] = avgR;
        newH[lowerIndex + 1] = avgH;

        for (int i = upperIndex; i < ringsCount; i++) {
            newR[i + 1] = ringsRadius[i];
            newH[i + 1] = ringHeights[i];
        }

        ringsCount = newCount;
        ringsRadius = newR;
        ringHeights = newH;
        body = null;
        MarkModified();
    }
}
