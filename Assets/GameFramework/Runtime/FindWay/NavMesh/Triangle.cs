using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public Vector2[] Points { get; } = new Vector2[3];
    public List<int> neighbors = new List<int>();

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        Points[0] = a;
        Points[1] = b;
        Points[2] = c;
    }

    public bool Contains(Vector2 point)
    {
        return PointInTriangle(point, Points[0], Points[1], Points[2]);
    }

    private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        return s >= 0 && t >= 0 && (s + t) <= 1;
    }

    public bool IsAdjacentTo(Triangle other)
    {
        int sharedVertices = 0;
        foreach (Vector2 p in Points)
        {
            foreach (Vector2 op in other.Points)
            {
                if (p == op) sharedVertices++;
            }
        }
        return sharedVertices >= 2;
    }

    public void AddNeighbor(int index) => neighbors.Add(index);

    public Vector2 Centroid() =>
        (Points[0] + Points[1] + Points[2]) / 3;
}