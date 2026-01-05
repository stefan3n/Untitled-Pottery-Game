using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public sealed class Potter : MonoBehaviour
{
    public int faces = 16;
    public float ringHeight = 0.1f;
    public int ringsCount = 16;

    public float[] ringsRadius = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };

    [Header("State")]
    public bool isStatic = false;
    private float[] defaultRadii;

    [Header("Materials")]
    public Material outsideMaterial;
    public Material insideMaterial;

    [Header("UV options")]
    public bool flipUInside = false;

    [Header("Radius Limits")]
    public float minRingRadius = 0.15f;
    public float maxRingRadius = 1.0f;

    Mesh mesh;
    Body body;
    MeshCollider meshCollider;

    void Awake()
    {
        if (ringsRadius == null || ringsRadius.Length != ringsCount)
        {
            UnityEngine.Debug.LogError("ringsRadius array must have exactly ringsCount elements!");
            return;
        }

        // Salvez starea initiala
        defaultRadii = (float[])ringsRadius.Clone();

        // Body primeste referinta la ringsRadius
        body = new Body(faces, ringsCount, ringHeight, ringsRadius);

        mesh = new Mesh { name = "Pot" };
        mesh.MarkDynamic();

        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        SetupMaterials();
    }

    private void SetupMaterials()
    {
        var mr = GetComponent<MeshRenderer>();
        // Daca sunt materiale setate din Inspector, le folosim, altfel Standard
        if (mr.sharedMaterials.Length == 0)
        {
            if (insideMaterial != null)
            {
                if (mr.sharedMaterial == null) mr.material = new Material(Shader.Find("Standard"));
                else mr.material = mr.sharedMaterial;
            }
            else
            {
                if (outsideMaterial == null) outsideMaterial = new Material(Shader.Find("Standard"));
                mr.materials = new[] { outsideMaterial, insideMaterial };
            }
        }
    }

    void Update()
    {
        if (isStatic) return;

        for (int i = 0; i < ringsRadius.Length; i++)
        {
            ringsRadius[i] = Mathf.Clamp(ringsRadius[i], minRingRadius, maxRingRadius);
        }

        GenerateMesh();
    }

    public float[] GetRadiiData()
    {
        return (float[])ringsRadius.Clone();
    }

    public void SetRadiiData(float[] newData)
    {
        if (newData.Length != ringsCount) return;

        System.Array.Copy(newData, ringsRadius, ringsCount);

        GenerateMesh();
    }

    public void ResetPot()
    {
        if (defaultRadii != null)
        {
            System.Array.Copy(defaultRadii, ringsRadius, ringsCount);
            GenerateMesh();
        }
    }

    public void GenerateMesh()
    {
        if (body == null) return;

        body.UpdateVertices();

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
                float v = ringsN > 1 ? y / (ringsN - 1f) : 0f;
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

        //Inside
        int offset = vCount;
        for (int i = 0; i < vCount; i++)
        {
            vertices[offset + i] = posOut[i];
            normals[offset + i] = -nrmOut[i];

            if (flipUInside)
            {
                uvs[offset + i] = new Vector2(1f - uvOut[i].x, uvOut[i].y);
            }
            else
            {
                uvs[offset + i] = uvOut[i];
            }
        }

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

        if (insideMaterial == null)
        {
            int[] allTris = new int[outsideTris.Length + insideTris.Length];
            System.Array.Copy(outsideTris, 0, allTris, 0, outsideTris.Length);
            System.Array.Copy(insideTris, 0, allTris, outsideTris.Length, insideTris.Length);
            mesh.subMeshCount = 1;
            mesh.triangles = allTris;
        }
        else
        {
            mesh.subMeshCount = 2;
            mesh.SetTriangles(outsideTris, 0);
            mesh.SetTriangles(insideTris, 1);
        }

        mesh.RecalculateBounds();

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
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