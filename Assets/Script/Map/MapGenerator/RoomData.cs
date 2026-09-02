using System.Collections.Generic;
using UnityEngine;


/**
 * @struct RoomBounds
 * @brief 각 테두리의 좌표
 */
public struct RoomBounds
{
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;
}

/**
 * @struct RoomPrimitive
 * @brief 방을 구성하는 축 정렬 사각형
 */
public struct RoomPrimitive
{
    public Vector2Int Offset;
    public int Width;
    public int Height;

    public RoomPrimitive(Vector2Int offset, int width, int height)
    {
        Offset = offset;
        Width = width;
        Height = height;
    }
}

/**
 * @class RoomData
 * @brief 스테이지의 방의 배치 정보를 저장하는 데이터 클래스
 */
public class RoomData
{
    public Vector2 Center { get; set; }
    public RoomBounds Bounds { get; set; }
    public List<Vector2> Doors { get; } = new List<Vector2>();
    public List<RoomPrimitive> Primitives { get; } = new List<RoomPrimitive>();
    public HashSet<Vector2Int> FloorCells { get; } = new HashSet<Vector2Int>();
    public int Width { get; set; }
    public int Height { get; set; }
    public int RoomID { get; set; }

    public RoomData()
    {
        Center = Vector2.zero;
    }
}


