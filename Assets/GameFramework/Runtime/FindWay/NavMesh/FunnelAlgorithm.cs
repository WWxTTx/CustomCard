using System.Collections.Generic;
using UnityEngine;

public class FunnelAlgorithm
{
    public static List<Vector3> SmoothPath(List<int> trianglePath, List<Triangle> triangles,
                                          Vector2 start, Vector2 end)
    {
        if (trianglePath.Count == 0)
            return new List<Vector3>();

        List<Vector2> portals = new List<Vector2>();
        portals.Add(start);

        // 获取所有通道边
        for (int i = 0; i < trianglePath.Count - 1; i++)
        {
            Triangle current = triangles[trianglePath[i]];
            Triangle next = triangles[trianglePath[i + 1]];

            List<Vector2> sharedEdge = GetSharedEdge(current, next);
            portals.Add(sharedEdge[0]);
            portals.Add(sharedEdge[1]);
        }

        portals.Add(end);

        // 漏斗算法
        List<Vector2> path = new List<Vector2>();
        path.Add(start);

        Vector2 apex = start;
        int leftIndex = 1;
        int rightIndex = 2;
        Vector2 leftPortal = portals[1];
        Vector2 rightPortal = portals[2];

        for (int i = 3; i < portals.Count; i += 2)
        {
            // 处理左侧
            if (Cross(apex, leftPortal, portals[i]) <= 0)
            {
                if (apex == leftPortal || Cross(apex, rightPortal, portals[i]) > 0)
                {
                    leftPortal = portals[i];
                    leftIndex = i;
                }
                else
                {
                    path.Add(rightPortal);
                    apex = rightPortal;

                    rightIndex = leftIndex + 1;
                    leftIndex = i;
                    rightPortal = portals[rightIndex];
                    leftPortal = portals[leftIndex];
                    i = leftIndex;
                }
                continue;
            }

            // 处理右侧
            if (Cross(apex, rightPortal, portals[i + 1]) >= 0)
            {
                if (apex == rightPortal || Cross(apex, leftPortal, portals[i + 1]) < 0)
                {
                    rightPortal = portals[i + 1];
                    rightIndex = i + 1;
                }
                else
                {
                    path.Add(leftPortal);
                    apex = leftPortal;

                    leftIndex = rightIndex - 1;
                    rightIndex = i + 1;
                    leftPortal = portals[leftIndex];
                    rightPortal = portals[rightIndex];
                    i = rightIndex - 1;
                }
            }
        }

        path.Add(end);
        return ConvertToVector3Path(path);
    }

    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    private static List<Vector2> GetSharedEdge(Triangle a, Triangle b)
    {
        List<Vector2> sharedEdge = new List<Vector2>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (a.Points[i] == b.Points[j])
                {
                    sharedEdge.Add(a.Points[i]);
                    if (sharedEdge.Count == 2) return sharedEdge;
                }
            }
        }
        return sharedEdge;
    }

    private static List<Vector3> ConvertToVector3Path(List<Vector2> path)
    {
        List<Vector3> result = new List<Vector3>();
        foreach (Vector2 p in path)
            result.Add(new Vector3(p.x, 0, p.y));
        return result;
    }
}