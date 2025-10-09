
using System;
using UnityEngine;

public class Delaunator
{
    private const float EPSILON = 1.192092896e-07f;
    private const int EDGE_STACK_SIZE = 512;

    public Vector2[] Coords { get; }
    public int[] Triangles { get; }
    public int[] Halfedges { get; }

    private int trianglesLen;
    private int hashSize;
    private int hullStart;
    private float cx, cy;
    private int[] hullPrev;
    private int[] hullNext;
    private int[] hullTri;
    private int[] hullHash;
    private int[] ids;
    private float[] dists;

    public Delaunator(Vector2[] points)
    {
        Coords = points;
        int n = points.Length;

        // 初始化数组
        int maxTriangles = Math.Max(2 * n - 5, 0);
        Triangles = new int[maxTriangles * 3];
        Halfedges = new int[maxTriangles * 3];

        hashSize = (int)Math.Ceiling(Math.Sqrt(n));
        hullPrev = new int[n];
        hullNext = new int[n];
        hullTri = new int[n];
        hullHash = new int[hashSize];
        ids = new int[n];
        dists = new float[n];

        // 执行三角剖分
        Triangulate();
    }

    private void Triangulate()
    {
        int n = Coords.Length;

        // 计算包围盒
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        for (int i = 0; i < n; i++)
        {
            Vector2 p = Coords[i];
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
            ids[i] = i;
        }

        // 选择初始点
        float cx = (minX + maxX) / 2;
        float cy = (minY + maxY) / 2;

        // 简化的三角剖分实现
        // (实际实现应包含完整的Delaunay三角剖分算法)

        // 这里仅返回一个简单三角剖分
        if (n >= 3)
        {
            Triangles[0] = 0;
            Triangles[1] = 1;
            Triangles[2] = 2;
            trianglesLen = 3;
        }
    }
}