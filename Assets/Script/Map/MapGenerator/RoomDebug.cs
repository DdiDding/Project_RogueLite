using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RoomDebug : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(6, 6);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(14, 12);
    [SerializeField] private int roomCount = 10;
    [SerializeField] private float pushStrength = 0.25f;
    [SerializeField] private float roomMargin = 10.0f;

    [Header("Gizmos")]
    [SerializeField] private Color roomColor = new Color(0.2f, 0.9f, 1.0f, 1.0f);
    [SerializeField] private Color centerColor = Color.yellow;
    [SerializeField] private bool drawRoomIDs = true;

    private readonly RoomGenerator generator = new RoomGenerator();
    private List<RoomData> rooms = new List<RoomData>();

    private void OnEnable()
    {
        rooms = generator.GenerateRooms(roomCount);
    }

    private void OnDrawGizmos()
    {
        if (rooms == null || rooms.Count == 0) return;

        foreach (RoomData room in rooms)
        {
            Gizmos.color = roomColor;

            Vector2 roomPosition = new Vector2(room.Center.x, room.Center.y);
            Vector2 roomSize = new Vector2(room.Width, room.Height);

            Gizmos.DrawWireCube(roomPosition, roomSize);

            Gizmos.color = centerColor;
            Gizmos.DrawSphere(roomPosition, 0.5f);
        }
    }

    private void generateRooms()
    {
        rooms.Clear();
        rooms = generator.GenerateRooms(roomCount);
    }
    private void stepOnce()
    {
        generator.ResolveOverlapStep((List<RoomData>)rooms, roomMargin, pushStrength);
    }
}
