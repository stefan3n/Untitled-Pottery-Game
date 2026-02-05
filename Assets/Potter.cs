using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public sealed class Potter : MonoBehaviour
{
    public int faces = 100;

    [Header("Vertical layout")]
    public float baseRingHeight = 0.05f;
    public float baseRingRadius = 0.3f;
    public int ringsCount = 8;

    public float maxPotHeight = 1.2f;

    [Header("Radius Limits")]
    public float minRingRadius = 0.05f;
    public float maxRingRadius = 0.8f;

    [HideInInspector] public float[] ringsRadius;
    [HideInInspector] public float[] ringHeights;

    [Header("State")]
    public bool isStatic = false;

    public bool IsGeometryLocked = false;

    private float[] defaultRadius;
    private float[] defaultRingHeights;

    [Header("UV options")]
    public bool flipUInside = false;

    [Header("Painting")]
    [SerializeField] private Material potPaintMaterial;
    [SerializeField] private int paintTextureSize = 1024;
    [SerializeField] private string paintTextureProperty = "_PaintTex";

    private Texture2D paintTexture;
    private Texture2D paintTextureInside;
    
    public string PaintTextureProperty => paintTextureProperty;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private int initialRingsCount;
    private bool isModified = false;

    Mesh mesh;
    Body body;
    MeshCollider meshCollider;
    void Awake()
    {
        initialRingsCount = ringsCount;
        
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

    private void ApplyTexturesToMaterials()
    {
        var mats = meshRenderer.materials;
        if (mats.Length > 0 && mats[0]) mats[0].SetTexture(paintTextureProperty, paintTexture);
        if (mats.Length > 1 && mats[1]) mats[1].SetTexture(paintTextureProperty, paintTextureInside);
        meshRenderer.materials = mats;
    }

    public void FullReset()
    {
        if (initialRingsCount > 0)
        {
            ringsCount = initialRingsCount;
        }

        ResetPot();

        body = null;
        GenerateMesh();

        Physics.SyncTransforms();

        SetPaintTexture();

        isModified = false;
    }
    
    public Texture2D GetPaintTexture(int submeshIndex = 0)
    {
        return submeshIndex == 1 ? paintTextureInside : paintTexture;
    }

    private void ClearTexture(Texture2D tex, Color c)
    {
        if (!tex) return;
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
        if (!tex) return;
        
        if (tex.height != newHeight)
        {
            tex.Reinitialize(paintTextureSize, newHeight);
        }
        
        ClearTexture(tex, Color.clear);
    }

    public bool CheckIfPainted()
    {
        if (!paintTexture) return false;
        Color[] pixels = paintTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i += 20)
        {
            if (pixels[i].a > 0.05f) return true; // Daca gasim ceva opac, e pictat
        }
        return false;
    }
    
    public void LoadPotData(float[] newRadii, float[] newHeights, Texture2D newTexOut, Texture2D newTexIn, bool shouldLockGeometry)
    {
        IsGeometryLocked = shouldLockGeometry;

        if (newRadii != null && newHeights != null)
        {
            ringsCount = newRadii.Length;
            ringsRadius = (float[])newRadii.Clone();
            ringHeights = (float[])newHeights.Clone();
            body = null;
            GenerateMesh();
        }

        void CopyToPotTexture(Texture2D source, ref Texture2D dest)
        {
            if (!source)
            {
                return; 
            }

            if (!dest) dest = CreatePaintTexture();

            if (dest.width != source.width || dest.height != source.height)
            {
                dest.Reinitialize(source.width, source.height);
            }

            if (source.isReadable)
            {
                dest.SetPixels(source.GetPixels());
                dest.Apply();
            }
            else
            {
                try { Graphics.CopyTexture(source, dest); } catch { }
            }
        }

        CopyToPotTexture(newTexOut, ref paintTexture);
        CopyToPotTexture(newTexIn, ref paintTextureInside);

        ApplyTexturesToMaterials();
        
        if (meshCollider) 
        {
            meshCollider.sharedMesh = null; 
            meshCollider.sharedMesh = mesh;
        }
    }

    public void ResetPot()
    {
        IsGeometryLocked = false;

        if (initialRingsCount > 0) ringsCount = initialRingsCount;
        ringsRadius = new float[ringsCount];
        ringHeights = new float[ringsCount];

        if (defaultRadius != null && defaultRadius.Length == ringsCount)
        {
            ringsRadius = (float[])defaultRadius.Clone();
            ringHeights = (float[])defaultRingHeights.Clone();
        }
        else
        {
            for (int i = 0; i < ringsCount; i++)
            {
                ringsRadius[i] = baseRingRadius;
                ringHeights[i] = i * baseRingHeight;
            }
        }

        ClearPaintTexture(Color.clear);
        body = null;
        isModified = true;
        GenerateMesh();
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

    public void MarkModified() => isModified = true;

    public float[] GetRadiiData() => (float[])ringsRadius.Clone();
    public float[] GetHeightsData() => (float[])ringHeights.Clone();

    public float GetTotalHeight()
    {
        if (ringHeights == null || ringHeights.Length == 0) return 0f;
        return ringHeights[ringsCount - 1] - ringHeights[0];
    }

    public void InsertRingBetween(int lowerIndex, int upperIndex)
    {
        if (IsGeometryLocked) return;

        if (lowerIndex < 0 || upperIndex >= ringsCount || (upperIndex - lowerIndex) != 1) return;

        int newCount = ringsCount + 1;
        float[] newR = new float[newCount];
        float[] newH = new float[newCount];

        float avgH = (ringHeights[lowerIndex] + ringHeights[upperIndex]) * 0.5f;
        float avgR = (ringsRadius[lowerIndex] + ringsRadius[upperIndex]) * 0.5f;

        for (int i = 0; i <= lowerIndex; i++)
        {
            newR[i] = ringsRadius[i];
            newH[i] = ringHeights[i];
        }

        newR[lowerIndex + 1] = avgR;
        newH[lowerIndex + 1] = avgH;
        
        for (int i = upperIndex; i < ringsCount; i++)
        {
            newR[i + 1] = ringsRadius[i];
            newH[i + 1] = ringHeights[i];
        }

        ringsCount = newCount;
        ringsRadius = newR;
        ringHeights = newH;
        body = null;
        MarkModified();
    }

    public void GenerateMesh()
    {
        if (body == null || body.vertices.GetLength(1) != ringsCount)
            body = new Body(faces, ringsCount, ringHeights, ringsRadius);

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
            float v = (totalH > 0) ? ringHeights[y] / totalH : 0f;
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

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
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

                trisOut[t] = bl; trisOut[t + 1] = tl; trisOut[t + 2] = br;
                trisOut[t + 3] = br; trisOut[t + 4] = tl; trisOut[t + 5] = tr;

                int off = vertexCountOneSide;
                trisIn[t] = off + bl; trisIn[t + 1] = off + br; trisIn[t + 2] = off + tl;
                trisIn[t + 3] = off + br; trisIn[t + 4] = off + tr; trisIn[t + 5] = off + tl;
                t += 6;
            }
        }

        mesh.SetTriangles(trisOut, 0);
        mesh.SetTriangles(trisIn, 1);
        mesh.RecalculateBounds();

        if (!meshCollider) meshCollider = GetComponent<MeshCollider>();
        if (meshCollider)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false; // Important pentru Raycast corect
            Physics.SyncTransforms();
        }
    }
}