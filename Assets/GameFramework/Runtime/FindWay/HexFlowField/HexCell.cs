// 六边形单元格类
using System.Collections.Generic;
using UnityEngine;
public class HexCell
{
    public Vector2Int coord;
    public Vector3 position;
    public int cost = int.MaxValue;
    public Vector3 bestDirection;
    public bool isObstacle;
    public List<HexCell> neighbors = new List<HexCell>();

    public HexCell(Vector2Int coord, Vector3 position)
    {
        this.coord = coord;
        this.position = position;
    }
}
