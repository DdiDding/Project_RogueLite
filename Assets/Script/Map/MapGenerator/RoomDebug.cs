using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/**
 * @class RoomDebug
 * @brief RoomGenerator의 시각적 디버깅을 위한 클래스
 */
public class RoomDebug : MonoBehaviour
{

    /**************************************************************************/
    // Private Values
    /**************************************************************************/
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
    [SerializeField] private Color connectionColor = Color.green;
    [SerializeField] private Color doorCandidateColor = Color.magenta;
    [SerializeField] private Color corridorColor = Color.red;
    [SerializeField] private Color floorCellColor = Color.white;
    [SerializeField] private Color wallCellColor = Color.blue;
    [SerializeField] private Color doorCellColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color entryCellColor = Color.cyan;
    [SerializeField] private bool drawRoomIDs = true;
    [SerializeField] private bool drawConnections = true;
    [SerializeField] private bool drawDoorCandidates = true;
    [SerializeField] private bool drawCorridors = true;
    [SerializeField] private bool drawFloorCells = true;
    [SerializeField] private bool drawWallCells = true;
    [SerializeField] private bool drawDoorCells = true;
    [SerializeField] private bool drawEntryCells = true;

    //@todo RoomGenerator 후에 k값 설정 제대로 하기
    private readonly RoomGenerator generator = new RoomGenerator(3);
    private List<RoomData> rooms = new List<RoomData>();
    private List<RoomGenerator.ConnectionEdge> connections = new List<RoomGenerator.ConnectionEdge>();
    private List<ConnectionPlan> connectionPlans = new List<ConnectionPlan>();
    private MapGridData mapGridData;
    private Coroutine mAutoResolveCoroutine;

    public MapGridData MapGridData => mapGridData;

    /**************************************************************************/
    // Private Functions
    /**************************************************************************/

    private void OnEnable()
    {
        rooms = generator.GenerateRooms(roomCount);
    }

    /**
     * @brief 컴포넌트가 비활성화 될때 실행되는 함수
     * @detail 컴포넌트가 비활성화될 때 코루틴 autoResolveCoroutine을 정리한다.
     */
    private void OnDisable()
    {
        stopAutoResolve();
    }


    /**
     * @brief Gizmos를 이용하여 Scene View에 방의 외곽선과 중심점을 그리는 함수
     * @detail Gizmo 렌더링이 필요할 때 Unity Editor에서 자동으로 호출하는 콜백함수다
     */
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


        // 간선 그리기
        if (drawConnections && connections != null)
        {
            Gizmos.color = connectionColor;

            foreach (RoomGenerator.ConnectionEdge connection in connections)
            {
                if (connection.mFrom < 0 || connection.mFrom >= rooms.Count ||
                    connection.mTo < 0 || connection.mTo >= rooms.Count)
                {
                    continue;
                }

                Gizmos.DrawLine(
                    rooms[connection.mFrom].Center,
                    rooms[connection.mTo].Center);
            }
        }

        // 문 후보 그리기
        if (drawDoorCandidates && connectionPlans != null)
        {
            Gizmos.color = doorCandidateColor;

            foreach (ConnectionPlan connectionPlan in connectionPlans)
            {
                Gizmos.DrawSphere(connectionPlan.FromDoorCandidate, 1.0f);
                Gizmos.DrawSphere(connectionPlan.ToDoorCandidate, 1.0f);
            }
        }

        // 복도 그리기
        if (drawCorridors && connectionPlans != null)
        {
            Gizmos.color = corridorColor;

            foreach (ConnectionPlan connectionPlan in connectionPlans)
            {
                foreach (Vector2Int corridorCell in connectionPlan.CorridorPath)
                {
                    Gizmos.DrawWireCube(new Vector3(corridorCell.x, corridorCell.y, 0f), Vector3.one);
                }
            }
        }

        if (drawFloorCells && mapGridData != null)
        {
            Gizmos.color = floorCellColor;

            for (int arrayY = 0; arrayY < mapGridData.Height; ++arrayY)
            {
                for (int arrayX = 0; arrayX < mapGridData.Width; ++arrayX)
                {
                    if (mapGridData.Cells[arrayX, arrayY] != MapCellType.Ground)
                    {
                        continue;
                    }

                    Vector3 cellPosition = new Vector3(
                        arrayX + mapGridData.Origin.x,
                        arrayY + mapGridData.Origin.y,
                        0f);
                    Gizmos.DrawWireCube(cellPosition, Vector3.one);
                }
            }
        }

        if (drawWallCells && mapGridData != null)
        {
            Gizmos.color = wallCellColor;

            for (int arrayY = 0; arrayY < mapGridData.Height; ++arrayY)
            {
                for (int arrayX = 0; arrayX < mapGridData.Width; ++arrayX)
                {
                    if (mapGridData.Cells[arrayX, arrayY] != MapCellType.Wall)
                    {
                        continue;
                    }

                    Vector3 cellPosition = new Vector3(
                        arrayX + mapGridData.Origin.x,
                        arrayY + mapGridData.Origin.y,
                        0f);
                    Gizmos.DrawWireCube(cellPosition, Vector3.one);
                }
            }
        }

        if (drawDoorCells && connectionPlans != null)
        {
            Gizmos.color = doorCellColor;

            foreach (ConnectionPlan plan in connectionPlans)
            {
                Gizmos.DrawWireCube(
                    new Vector3(plan.FromDoorCell.x, plan.FromDoorCell.y, 0f),
                    Vector3.one * 0.6f);
                Gizmos.DrawWireCube(
                    new Vector3(plan.ToDoorCell.x, plan.ToDoorCell.y, 0f),
                    Vector3.one * 0.6f);
            }
        }

        if (drawEntryCells && connectionPlans != null)
        {
            Gizmos.color = entryCellColor;

            foreach (ConnectionPlan plan in connectionPlans)
            {
                Gizmos.DrawWireCube(
                    new Vector3(plan.FromEntryCell.x, plan.FromEntryCell.y, 0f),
                    Vector3.one * 0.4f);
                Gizmos.DrawWireCube(
                    new Vector3(plan.ToEntryCell.x, plan.ToEntryCell.y, 0f),
                    Vector3.one * 0.4f);
            }
        }
    }


    /**
     * @brief 현재 설정값을 기준으로 방 데이터를 다시 생성한다.
     *
     * @details
     * 기존 방 목록을 버리고 RoomGenerator에서 새 방 목록을 받아온다.
     */
    private void generateRooms()
    {
        rooms.Clear();
        connections.Clear();
        connectionPlans.Clear();
        mapGridData = null;
        rooms = generator.GenerateRooms(roomCount);
    }

    /**
     * @brief 현재 방 배치를 기준으로 최소 신장 트리 간선을 생성한다.
     */
    private void generateConnections()
    {
        generator.SnapRoomsToGrid(rooms);
        connections = generator.LinkRoom(rooms);
        connectionPlans.Clear();
        mapGridData = null;
    }

    /**
     * @brief 현재 연결 간선을 기준으로 문 후보 계획을 생성한다.
     */
    private void generateDoorCandidates()
    {
        connectionPlans = generator.CreateDoorCandidates(rooms, connections);
        generator.DetermineDoorSides(rooms, connectionPlans);
        generator.CreateDoorAndEntryCells(rooms, connectionPlans);
        generator.CreateSameAxisCorridorWaypoints(connectionPlans);
        generator.CreateCorridorPaths(connectionPlans);
        mapGridData = null;
    }

    private void generateMapCells()
    {
        generator.SnapRoomsToGrid(rooms);
        mapGridData = generator.CreateEmptyMapGridData(rooms, connectionPlans);
        generator.FillRoomCells(mapGridData, rooms);
        generator.FillConnectionCells(mapGridData, connectionPlans);
        generator.CreateWallCells(mapGridData);
    }

    /**
     * @brief 겹친 방을 밀어내는 과정을 한 번만 수행한다.
     */
    private bool stepOnce()
    {
        bool hasOverlap = generator.ResolveOverlapStep(rooms, roomMargin, pushStrength);

        if (hasOverlap)
        {
            connections.Clear();
            connectionPlans.Clear();
            mapGridData = null;
        }

        return hasOverlap;
    }

    /**
     * @brief 겹친 방이 없을 때까지 방을 밀어낸다.
     * @detail 코루틴을 사용하여 stepOnce을 주기적으로 실행한다.
     */
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

    /**
     * @brief 실행 중인 자동 겹침 해소 Coroutine을 중지, 그후 방을 그리드에 맞춘다.
     */
    private void stopAutoResolve()
    {
        if (mAutoResolveCoroutine == null)
        {
            return;
        }

        StopCoroutine(mAutoResolveCoroutine);
        mAutoResolveCoroutine = null;

        generator.SnapRoomsToGrid(rooms);
        mapGridData = null;
    }


    /**
     * @brief 일정 간격으로 방 겹침 해소를 반복 수행한다.
     *
     * @details
     * 각 반복마다 stepOnce를 호출한다.
     * 더 이상 겹침이 없거나 maxAutoResolveSteps에 도달하면 종료한다.
     *
     * @return Coroutine 실행을 위한 IEnumerator
     */
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
