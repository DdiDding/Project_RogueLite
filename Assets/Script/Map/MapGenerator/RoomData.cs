using UnityEngine;

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
