using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class RoomDebug : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(6, 6);
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(14, 12);
    [SerializeField] private int roomCount = 10;
    [SerializeField] private float pushStrength = 0.25f;
    [SerializeField] private float roomMargin = 10.0f;

    [Header("Auto Resolve")]
    [SerializeField] private float autoResolveInterval = 0.05f;
    [SerializeField] private int maxAutoResolveSteps = 100;

    [Header("Gizmos")]
    [SerializeField] private Color roomColor = new Color(0.2f, 0.9f, 1.0f, 1.0f);
    [SerializeField] private Color centerColor = Color.yellow;
    [SerializeField] private bool drawRoomIDs = true;

    private readonly RoomGenerator generator = new RoomGenerator();
    private List<RoomData> rooms = new List<RoomData>();
    private Coroutine mAutoResolveCoroutine;

    private void OnEnable()
    {
        rooms = generator.GenerateRooms(roomCount);
    }

    private void OnDisable()
    {
        stopAutoResolve();
    }

    private void OnDrawGizmos()
    {
        if (rooms == null || rooms.Count == 0)
        {
            return;
        }

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

    private bool stepOnce()
    {
        return generator.ResolveOverlapStep(rooms, roomMargin, pushStrength);
    }

    private void startAutoResolve()
    {
        if (Application.isPlaying == false)
        {
            Debug.LogWarning("Auto Resolve uses Coroutine, so enter Play Mode first.");
            return;
        }

        stopAutoResolve();
        mAutoResolveCoroutine = StartCoroutine(autoResolveCoroutine());
    }

    private void stopAutoResolve()
    {
        if (mAutoResolveCoroutine == null)
        {
            return;
        }

        StopCoroutine(mAutoResolveCoroutine);
        mAutoResolveCoroutine = null;
    }

    private IEnumerator autoResolveCoroutine()
    {
        for (int stepIndex = 0; stepIndex < maxAutoResolveSteps; ++stepIndex)
        {
            bool bHasOverlap = stepOnce();

            if (bHasOverlap == false)
            {
                break;
            }

            yield return new WaitForSeconds(autoResolveInterval);
        }

        mAutoResolveCoroutine = null;
    }
}
