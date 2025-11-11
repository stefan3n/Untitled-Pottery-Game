using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class Potter : MonoBehaviour
{
	public int faces = 10;
	public float ringHeight = 0.5f;
	public int ringsCount = 5;
	public float[] ringsRadius = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };

	Mesh mesh;
	Body body;

	void Awake()
	{
		if (ringsRadius == null || ringsRadius.Length != ringsCount)
		{
			Debug.LogError("ringsRadius array must have exactly ringsCount elements!");
			return;
		}

		body = new Body(faces, ringsCount, ringHeight, ringsRadius);
		mesh = new Mesh() { name = "Pot" };
		GetComponent<MeshFilter>().sharedMesh = mesh; 
		mesh.MarkDynamic();
		
		var renderer = GetComponent<MeshRenderer>();
		if (renderer.sharedMaterial == null)
		{
			renderer.material = new Material(Shader.Find("Standard"));
		}
	}
	
	void Update()
	{
		body.UpdateVertices();
		
		mesh.vertices = body.VerticesToPositionArray();
		mesh.normals = body.VerticesToNormalsArray();

		int facesN = body.vertices.GetLength(0);
		int ringsN = body.vertices.GetLength(1);

		int[] triangles = new int[6 * (facesN - 1) * (ringsN - 1)];
		int t = 0;
		for (int y = 0; y < ringsN - 1; y++)
		{
			for (int x = 0; x < facesN - 1; x++)
			{
				int a = body.vertices[x, y].index;
				int b = body.vertices[x, y + 1].index;
				int c = body.vertices[x + 1, y + 1].index;
				int d = body.vertices[x + 1, y].index;

				triangles[t++] = a; triangles[t++] = b; triangles[t++] = c;
				triangles[t++] = a; triangles[t++] = c; triangles[t++] = d;
			}
		}
		mesh.triangles = triangles;

		Vector2[] uvs = new Vector2[facesN * ringsN];
		for (int y = 0; y < ringsN; y++)
		{
			for (int x = 0; x < facesN; x++)
			{
				int i = x + y * facesN;
				float u = facesN > 1 ? x / (facesN - 1f) : 0f;
				float v = ringsN > 1 ? y / (ringsN - 1f) : 0f;
				uvs[i] = new Vector2(u, v);
			}
		}
		mesh.uv = uvs;

		mesh.MarkModified();
	}
	
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (mesh == null || body == null)
			return;

		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.red;
		Gizmos.DrawWireMesh(mesh);
		Gizmos.color = Color.cyan;

		foreach (Vertex vertex in body.vertices)
			Gizmos.DrawSphere(vertex.position, 0.015f);
	}
#endif
}