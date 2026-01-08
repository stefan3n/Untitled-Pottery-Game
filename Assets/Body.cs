﻿using System;
using UnityEngine;

public sealed class Body
{
    public readonly Vertex[,] vertices;

    readonly float[] ringsRadius;
    readonly float[] ringHeights;
    readonly int ringsCount;
    readonly int faces;

    public Body(int faces, int ringsCount, float[] ringHeights, float[] ringsRadius)
    {
        if (faces < 2) throw new ArgumentOutOfRangeException(nameof(faces));
        if (ringsCount < 2) throw new ArgumentOutOfRangeException(nameof(ringsCount));
        if (ringsRadius.Length != ringsCount) throw new ArgumentException(nameof(ringsRadius));
        if (ringHeights.Length != ringsCount) throw new ArgumentException(nameof(ringHeights));

        this.ringsCount = ringsCount;
        this.faces = faces;

        this.ringsRadius = ringsRadius;
        this.ringHeights = ringHeights;

        vertices = new Vertex[faces, ringsCount];
    }

    public void UpdateVertices()
    {
        int i = 0;
        for (int y = 0; y < ringsCount; y++)
        {
            float ringY = ringHeights[y];

            for (int x = 0; x < faces; x++)
            {
                float angle = Mathf.PI * 2f / (faces - 1) * x;
                float posX = Mathf.Cos(angle);
                float posZ = Mathf.Sin(angle);

                Vector3 position = new(posX * ringsRadius[y], ringY, posZ * ringsRadius[y]);
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