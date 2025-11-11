using System;
using UnityEngine;

public sealed class Body
{
	public readonly Vertex[,] vertices;

	readonly float[] ringsRadius;
	readonly float ringHeight;
	readonly int ringsCount;
	readonly int faces;

	public Body(int faces, int ringsCount, float ringHeight, float[] ringsRadius)
	{
		if (faces < 2) throw new ArgumentOutOfRangeException(nameof(faces));
		if (ringsCount < 2) throw new ArgumentOutOfRangeException(nameof(ringsCount));
		if (ringsRadius.Length != ringsCount) throw new ArgumentException(nameof(ringsRadius));

		this.ringsCount = ringsCount;
		this.ringHeight = Mathf.Abs(ringHeight);
		this.faces = faces;
		this.ringsRadius = ringsRadius;
		
		vertices = new Vertex[faces, ringsCount];
	}
	
	public void UpdateVertices()
	{
		int i = 0;
		for (int y = 0; y < ringsCount; y++)
		{
			for (int x = 0; x < faces; x++)
			{
				float posX = Mathf.Cos(Mathf.PI * 2f / (faces - 1) * x);
				float posY = y * ringHeight;
				float posZ = Mathf.Sin(Mathf.PI * 2f / (faces - 1) * x);

				Vector3 position = new(posX * ringsRadius[y], posY, posZ * ringsRadius[y]);
				Vector3 normal = new Vector3(position.x, 0, position.z).normalized;

				vertices[x, y] = new Vertex(position, normal, i);
				i++;
			}
		}
	}
	
	public Vector3[] VerticesToPositionArray()
	{
		Vector3[] result = new Vector3[ringsCount * faces];

		int i = 0;
		for (int y = 0; y < ringsCount; y++)
		{
			for (int x = 0; x < faces; x++)
			{
				result[i] = vertices[x, y].position;
				i++;
			}
		}

		return result;
	}
	
	public Vector3[] VerticesToNormalsArray()
	{
		Vector3[] result = new Vector3[ringsCount * faces];
		
		int i = 0;
		for (int y = 0; y < ringsCount; y++)
		{
			for (int x = 0; x < faces; x++)
			{
				result[i] = vertices[x, y].normal;
				i++;
			}
		}

		return result;
	}
}