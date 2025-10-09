using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 三角形导航网格生成 适用于少量单位 动态起点和终点的路径寻路
/// </summary>
public class NavMeshGenerator : MonoBehaviour
{
    public Transform[] points;
    private Delaunator delaunay;
    private List<Triangle> triangles = new List<Triangle>();

    void Start()
    {
        GenerateNavMesh();
    }

    public void GenerateNavMesh()
    {
        Vector2[] coords = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pos = points[i].position;
            coords[i] = new Vector2(pos.x, pos.z);
        }

        delaunay = new Delaunator(coords);
        triangles.Clear();
        
        int[] trianglesArr = delaunay.Triangles;
        for (int i = 0; i < trianglesArr.Length; i += 3)
        {
            triangles.Add(new Triangle(
                coords[trianglesArr[i]],
                coords[trianglesArr[i + 1]],
                coords[trianglesArr[i + 2]]
            ));
        }
        
        BuildAdjacency(); // 构建三角形邻接关系
    }

    // 构建三角形邻接关系
    private void BuildAdjacency()
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            for (int j = i + 1; j < triangles.Count; j++)
            {
                if (triangles[i].IsAdjacentTo(triangles[j]))
                {
                    triangles[i].AddNeighbor(j);
                    triangles[j].AddNeighbor(i);
                }
            }
        }
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        Vector2 startPos = new Vector2(start.x, start.z);
        Vector2 endPos = new Vector2(end.x, end.z);

        int startTri = FindContainingTriangle(startPos);
        int endTri = FindContainingTriangle(endPos);

        if (startTri == -1 || endTri == -1)
            return new List<Vector3>();

        List<int> trianglePath = FindTrianglePath(startTri, endTri);
        return FunnelAlgorithm.SmoothPath(trianglePath, triangles, startPos, endPos);
    }

    private int FindContainingTriangle(Vector2 point)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].Contains(point))
                return i;
        }
        return -1;
    }

    // A*寻路算法实现
    private List<int> FindTrianglePath(int start, int end)
    {
        List<int> path = new List<int>();
        if (start == end)
        {
            path.Add(start);
            return path;
        }

        Dictionary<int, float> gScore = new Dictionary<int, float>();
        Dictionary<int, float> fScore = new Dictionary<int, float>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        List<int> openSet = new List<int>();

        for (int i = 0; i < triangles.Count; i++)
        {
            gScore[i] = float.MaxValue;
            fScore[i] = float.MaxValue;
        }

        gScore[start] = 0;
        fScore[start] = Vector2.Distance(
            triangles[start].Centroid(),
            triangles[end].Centroid()
        );

        openSet.Add(start);

        while (openSet.Count > 0)
        {
            int current = openSet[0];
            float minF = fScore[current];

            // 寻找fScore最小的节点
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fScore[openSet[i]] < minF)
                {
                    current = openSet[i];
                    minF = fScore[current];
                }
            }

            if (current == end)
            {
                // 重建路径
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            openSet.Remove(current);

            foreach (int neighbor in triangles[current].neighbors)
            {
                float tentativeG = gScore[current] + 
                    Vector2.Distance(
                        triangles[current].Centroid(),
                        triangles[neighbor].Centroid()
                    );

                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = gScore[neighbor] + 
                        Vector2.Distance(
                            triangles[neighbor].Centroid(),
                            triangles[end].Centroid()
                        );

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return path;
    }

    void OnDrawGizmos()
    {
        if (delaunay == null) return;

        Gizmos.color = Color.cyan;
        int[] trianglesArr = delaunay.Triangles;
        for (int i = 0; i < trianglesArr.Length; i += 3)
        {
            Vector3 p1 = ToVector3(delaunay.Coords[trianglesArr[i]]);
            Vector3 p2 = ToVector3(delaunay.Coords[trianglesArr[i + 1]]);
            Vector3 p3 = ToVector3(delaunay.Coords[trianglesArr[i + 2]]);

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p1);
        }
    }

    private Vector3 ToVector3(Vector2 v) => new Vector3(v.x, 0, v.y);
}




