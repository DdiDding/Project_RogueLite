using UnityEngine;

/**
 * @class RoomData
 * @brief 스테이지의 방의 배치 정보를 저장하는 데이터 클래스
 */
public class RoomData
{
    public Vector2 Center { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int RoomID { get; set; }

    public RoomData()
    {
        Center = Vector2.zero;
    }
}
