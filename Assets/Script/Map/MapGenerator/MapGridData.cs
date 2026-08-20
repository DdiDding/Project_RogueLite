using System.Collections.Generic;
using UnityEngine;

public enum MapCellType
{
    Empty,
    Ground,
    Wall
}


public class MapGridData
{
    public Vector2Int Origin { get; }
    public MapCellType[,] Cells { get; }
    // 좌표 중복 막기위한 해시
    public HashSet<Vector2Int> DoorCells { get; } = new HashSet<Vector2Int>();

    // 프로퍼티!
    public int Width => Cells.GetLength(0);
    public int Height => Cells.GetLength(1);

    public MapGridData(Vector2Int origin, int width, int height)
    {
        Origin = origin;
        Cells = new MapCellType[width, height];
    }
}
