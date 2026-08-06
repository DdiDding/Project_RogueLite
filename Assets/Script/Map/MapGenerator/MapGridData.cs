using System.Collections.Generic;
using UnityEngine;

public enum MapCellType
{
    Empty,
    Floor,
    Wall
}

public class MapGridData
{
    public Vector2Int Origin { get; }
    public MapCellType[,] Cells { get; }
    public HashSet<Vector2Int> DoorCells { get; } = new HashSet<Vector2Int>();

    public int Width => Cells.GetLength(0);
    public int Height => Cells.GetLength(1);

    public MapGridData(Vector2Int origin, int width, int height)
    {
        Origin = origin;
        Cells = new MapCellType[width, height];
    }
}
