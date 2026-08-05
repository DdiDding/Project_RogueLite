using System.Collections.Generic;
using UnityEngine;

public enum DoorSide
{
    Unknown,
    Left,
    Right,
    Bottom,
    Top
}

/**
 * @class ConnectionPlan
 * @brief 두 방을 연결할 문 후보 쌍을 저장하는 데이터 클래스
 */
public class ConnectionPlan
{
    public int FromRoomID { get; }
    public int ToRoomID { get; }
    public Vector2 FromDoorCandidate { get; set; }
    public Vector2 ToDoorCandidate { get; set; }

    public Vector2Int FromDoorCell { get; set; }
    public Vector2Int ToDoorCell { get; set; }

    // 문 후보가 어느 방의 어느 면에 위치하는지 저장
    public DoorSide FromDoorSide { get; set; }
    public DoorSide ToDoorSide { get; set; }

    // 복도 중심선을 구성하는 시작점, 꺾임점, 끝점
    public List<Vector2Int> CorridorWaypoints { get; } = new List<Vector2Int>();

    // 각 방을 연결하는 복도 경로를 저장하는 리스트
    public List<Vector2Int> CorridorPath { get; } = new List<Vector2Int>();

    public ConnectionPlan(
        int fromRoomID,
        int toRoomID,
        Vector2 fromDoorCandidate,
        Vector2 toDoorCandidate)
    {
        FromRoomID = fromRoomID;
        ToRoomID = toRoomID;
        FromDoorCandidate = fromDoorCandidate;
        ToDoorCandidate = toDoorCandidate;
    }
}
