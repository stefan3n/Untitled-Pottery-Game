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

<<<<<<< Updated upstream
    [Header("Radius Limits")]
    public float minRingRadius = 0.15f;
    public float maxRingRadius = 0.9f;

=======
>>>>>>> Stashed changes
    [Header("Painting")]
    [SerializeField] private Material potPaintMaterial;
    [SerializeField] private int paintTextureSize = 1024;
    [SerializeField] private string paintTextureProperty = "_PaintTex";

<<<<<<< Updated upstream
    private Texture2D paintTexture;      
    private Texture2D paintTextureInside;  
    
    private MeshRenderer meshRenderer;
=======
    private Texture2D paintTexture;
    private Texture2D paintTextureInside;
>>>>>>> Stashed changes

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private int initialRingsCount;
    private bool isModified = false;

    Mesh mesh;
    Body body;

    void Awake()
    {
<<<<<<< Updated upstream
        if (baseRingHeight <= 0.0f || baseRingRadius <= 0.0f || ringsCount <= 2 || maxPotHeight < baseRingHeight * (ringsCount - 1))
=======
        initialRingsCount = ringsCount;

>>>>>>> Stashed changes
        mesh = new Mesh();
        mesh.name = "PotMesh";
        mesh.MarkDynamic();

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        if (potPaintMaterial != null)
        {
            meshRenderer.materials = new[]
            {
                new Material(potPaintMaterial),
                new Material(potPaintMaterial)
            };
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
        paintTexture = CreatePaintTexture();
        paintTextureInside = CreatePaintTexture();
        ApplyTexturesToMaterials();

        bool needsInit = (ringsRadius == null || ringsRadius.Length != ringsCount || ringHeights == null || ringHeights.Length != ringsCount);

        if (needsInit)
        {
            ringsRadius = new float[ringsCount];
            ringHeights = new float[ringsCount];
            for (int i = 0; i < ringsCount; i++)
            {
                ringsRadius[i] = baseRingRadius;
                ringHeights[i] = i * baseRingHeight;
            }
        }

        defaultRadius = (float[])ringsRadius.Clone();
        defaultRingHeights = (float[])ringHeights.Clone();

        GenerateMesh();
    }


    private Texture2D CreatePaintTexture()
    {
        var tex = new Texture2D(paintTextureSize, paintTextureSize, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);
        return tex;
    }

    private void ApplyTexturesToMaterials()
    {
<<<<<<< Updated upstream
        isModified = true;
    }

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
        meshRenderer.materials = mats;
    }

    private void ResizeAndClear(Texture2D tex, int newHeight)
    {
        if (tex == null) return;
        if (tex.height != newHeight)
        {
            tex.Reinitialize(paintTextureSize, newHeight);
            ClearTexture(tex, Color.clear);
        }
=======
        var mats = meshRenderer.materials;
        if (mats.Length > 0 && mats[0] != null) mats[0].SetTexture(paintTextureProperty, paintTexture);
        if (mats.Length > 1 && mats[1] != null) mats[1].SetTexture(paintTextureProperty, paintTextureInside);
        meshRenderer.materials = mats;
    }

    public Texture2D GetPaintTexture(int submeshIndex = 0)
    {
        return submeshIndex == 1 ? paintTextureInside : paintTexture;
>>>>>>> Stashed changes
    }

    public void MarkModified()
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

    public void SetPaintTexture()
    {
        float h = GetTotalHeight();
        float avgRadius = 0f;
        for (int i = 0; i < ringsCount; i++) avgRadius += ringsRadius[i];
        if (ringsCount > 0) avgRadius /= ringsCount;

        float circumference = 2f * Mathf.PI * avgRadius;
        if (circumference > 0.0001f)
        {
            int newHeight = Mathf.RoundToInt(paintTextureSize * (h / circumference));
            newHeight = Mathf.Clamp(newHeight, 16, 8192);

            if (paintTexture.height != newHeight) paintTexture.Reinitialize(paintTextureSize, newHeight);
            if (paintTextureInside.height != newHeight) paintTextureInside.Reinitialize(paintTextureSize, newHeight);
        }
    }

    public bool CheckIfPainted()
    {
        if (paintTexture == null) return false;
        Color[] pixels = paintTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i += 20)
        {
            if (pixels[i].a > 0.05f) return true; 
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


        if (newTexOut != null)
        {
            if (paintTexture == null) paintTexture = CreatePaintTexture();

            if (paintTexture.width != newTexOut.width || paintTexture.height != newTexOut.height)
            {
                paintTexture.Reinitialize(newTexOut.width, newTexOut.height);
            }

            Graphics.CopyTexture(newTexOut, paintTexture);
        }
        else
        {
            ClearTexture(paintTexture, Color.clear);
        }

        if (newTexIn != null)
        {
            if (paintTextureInside == null) paintTextureInside = CreatePaintTexture();

            if (paintTextureInside.width != newTexIn.width || paintTextureInside.height != newTexIn.height)
            {
                paintTextureInside.Reinitialize(newTexIn.width, newTexIn.height);
            }

            Graphics.CopyTexture(newTexIn, paintTextureInside);
        }
        else
        {
            ClearTexture(paintTextureInside, Color.clear);
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

        Vector3[] posOut = body.VerticesToPositionArray();
        Vector3[] nrmOut = body.VerticesToNormalsArray();

<<<<<<< Updated upstream
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
=======
        Vector3[] vertices = new Vector3[totalVertices];
        Vector3[] normals = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];
        float totalH = GetTotalHeight();
>>>>>>> Stashed changes

                float totalH = GetTotalHeight();
                float currentH = ringHeights[y] - ringHeights[0];
                float v = (totalH > 0) ? currentH / totalH : 0f;

                uvOut[i] = new Vector2(u, v);
            }
        }

        // Outside
        for (int i = 0; i < vCount; i++)
        {
            vertices[vertexCountOneSide + i] = vOut[i];
            normals[vertexCountOneSide + i] = -nOut[i];
        }

        // Inside
        int offset = vCount;
        for (int i = 0; i < vCount; i++)
        {
<<<<<<< Updated upstream
            float v = ringHeights[y] / totalH;

            if (flipUInside)
                uvs[offset + i] = new Vector2(1f - uvOut[i].x, uvOut[i].y);
            else
                uvs[offset + i] = uvOut[i];
        }

        mesh.Clear();
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;

        // Triangles
=======
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
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
                // Outside (CCW)
                trisOut[t] = bl;
                trisOut[t + 1] = tl;
                trisOut[t + 2] = br;
                trisOut[t + 3] = br;
                trisOut[t + 4] = tl;
                trisOut[t + 5] = tr;
                
=======
                trisOut[t] = bl; trisOut[t + 1] = tl; trisOut[t + 2] = br;
                trisOut[t + 3] = br; trisOut[t + 4] = tl; trisOut[t + 5] = tr;

>>>>>>> Stashed changes
                int off = vertexCountOneSide;
                trisIn[t] = off + bl; trisIn[t + 1] = off + br; trisIn[t + 2] = off + tl;
                trisIn[t + 3] = off + br; trisIn[t + 4] = off + tr; trisIn[t + 5] = off + tl;
                t += 6;
            }
        }

        mesh.SetTriangles(trisOut, 0);
        mesh.SetTriangles(trisIn, 1);
<<<<<<< Updated upstream

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
=======
        mesh.RecalculateBounds();

        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false; 
            Physics.SyncTransforms();
>>>>>>> Stashed changes
        }
    }
<<<<<<< Updated upstream

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
}
=======
}
>>>>>>> Stashed changes
