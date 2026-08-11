using UnityEngine;
using System.Collections.Generic;
/**
 * @class RoomGenerator
 * @brief 스테이지의 방들을 생성,배치 및 연결하는 클래스
 * @detail
 * 맵을 생성하는 과정중 하나로, 맵 생성이 오래걸리기 때문에 후에 worker thread에서 동작할 예정이다.
 */
public class RoomGenerator
{
    /**************************************************************************/
    // Private Values
    /**************************************************************************/

    /**
     * @brief K-Nearest이용한 후보 간선 생성 시 각 방이 선택할 인접 방 개수
     */
    private readonly int mNearestNeighborAmount;

    /**************************************************************************/
    // Private Struct
    /**************************************************************************/
   
    /**
     * @struct Edge
     * @brief 방 간의 연결된 간선을 의미하는 구조체
     */
    private struct Edge
    {
        public int mRoomId;
        public float mDistance;

        public Edge(int roomId, float distance)
        {
            mRoomId = roomId;
            mDistance = distance;
        }
    }

    /**
     * @struct ConnectionEdge
     * @brief 크루스칼 알고리즘에 사용하기 위한 간선 구조체
     */
    public struct ConnectionEdge
    {
        public int mFrom;
        public int mTo;
        public float mDistance;

        public ConnectionEdge(int from, int to, float distance)
        {
            mFrom = from;
            mTo = to;
            mDistance = distance;
        }
    }

    /**
     * @class UnionFind
     * @brief 크루스칼 알고리즘에서 사이클을 방지하기 위해 사용하는 클래스
     */
    private class UnionFind
    {
        private readonly int[] mParent;
        private readonly int[] mRank;

        public UnionFind(int count)
        {
            mParent = new int[count];
            mRank = new int[count];

            for (int i = 0; i < count; ++i)
            {
                mParent[i] = i;
            }
        }

        public int Find(int node)
        {
            if (mParent[node] != node)
            {
                mParent[node] = Find(mParent[node]);
            }

            return mParent[node];
        }

        public bool Union(int a, int b)
        {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA == rootB)
            {
                return false;
            }

            if (mRank[rootA] < mRank[rootB])
            {
                mParent[rootA] = rootB;
            }
            else if (mRank[rootA] > mRank[rootB])
            {
                mParent[rootB] = rootA;
            }
            else
            {
                mParent[rootB] = rootA;
                ++mRank[rootA];
            }

            return true;
        }
    }

    /**************************************************************************/
    // Public Functions
    /**************************************************************************/

    public RoomGenerator(int nearestNeighborAmount)
    {
        mNearestNeighborAmount = nearestNeighborAmount;
    }

    /**
     * @brief 방을 랜덤으로 생성한다.
     * 
     * @param roomQuantity 생성할 방의 개수
     * @return 생성된 방들을 담은 리스트
     */
    public List<RoomData> GenerateRooms(int roomQuantity)
    {
        List<RoomData> rooms = new List<RoomData>();

        for (int i = 0; i < roomQuantity; ++i)
        {
            RoomData room = new RoomData();
            room.RoomID = i;
            room.Center = Vector2.zero;

            room.Height = Random.Range(10, 30); /*int만 생성*/
            bool isHeightEven = room.Height % 2 == 0;
            if (isHeightEven) room.Height += 1; /*홀수로 만들어주기*/

            room.Width = Random.Range(10, 30);
            bool isEvenWidth = room.Width % 2 == 0;
            if (isEvenWidth) room.Width += 1; /*홀수로 만들어주기*/

            rooms.Add(room);
        }

        return rooms;
    }


    /**
     * @brief 겹친 방들이 없을 때까지 반복적으로 방을 밀어낸다.
     * 
     * @param rooms 밀어낼 방들의 리스트
     * @param roomMargin 각 방끼리의 margin값
     * @param pushStrength 방이 겹쳤을 때 밀어내는 강도
     * @return void
     */
    public void ResolveOverlap(List<RoomData> rooms, float roomMargin, float pushStrength)
    {
        while (ResolveOverlapStep(rooms, roomMargin, pushStrength) == true) { };
    }


    /**
     * @brief 겹친 방들있다면 한 번 방을 밀어내는 연산을 수행한다.
     * @details
     * 모든 방 쌍을 검사하여 AABB가 겹치는 경우 이동량을 누적한다.
     * 이동량은 모든 충돌 검사가 끝난 뒤 한 번에 적용한다.
     * 한 번만 밀어내는 연산을 수행하기 때문에 연산 후 겹치는 방이 존재할 수 있다.
     * 
     * @param rooms 밀어낼 방들의 리스트
     * @param roomMargin 방 크기에 추가로 반영할 여백
     * @param pushStrength 방이 겹쳤을 때 밀어내는 강도
     * @return bool 겹친 방이 존재했었다면 true, 겹친 방이 하나도 없었으면 false 반환
     */
    public bool ResolveOverlapStep(List<RoomData> rooms, float roomMargin, float pushStrength)
    {
        bool isOverlap = false;
        Vector2[] moveAmount = new Vector2[rooms.Count];

        for (int i = 0; i < rooms.Count; ++i)
        {
            for(int j = i + 1; j < rooms.Count; ++j)
            {
                if (aabb(rooms[i], rooms[j], roomMargin) == false) continue;
                isOverlap = true;

                RoomData roomA = rooms[i];
                RoomData roomB = rooms[j];

                float overlapX = (roomA.Width * 0.5f + roomB.Width * 0.5f + roomMargin - Mathf.Abs(roomA.Center.x - roomB.Center.x)) * pushStrength;
                float overlapY = (roomA.Height * 0.5f + roomB.Height * 0.5f + roomMargin - Mathf.Abs(roomA.Center.y - roomB.Center.y)) * pushStrength;

                // 더 적게 겹친 축으로 밀어내 이동량을 줄임
                if (overlapX < overlapY)
                {
                    float roomAPushAmount = roomA.Center.x < roomB.Center.x ? -overlapX : overlapX;
                    moveAmount[i].x += roomAPushAmount;
                    moveAmount[j].x -= roomAPushAmount;
                }
                else
                {
                    float roomAPushAmount = roomA.Center.y < roomB.Center.y ? -overlapY : overlapY;
                    moveAmount[i].y += roomAPushAmount;
                    moveAmount[j].y -= roomAPushAmount;
                }
            }
        }

        if (isOverlap == false) return false;

        for (int i = 0; i < rooms.Count; ++i)
        {
            rooms[i].Center += moveAmount[i];
        }
        return true;
    }

    /**
     * 방들을 정수로 떨어지게 하는 함수
     * margin이 어느정도 있어서 겹치지는 않을 것 같음
     */
    public void SnapRoomsToGrid(List<RoomData> rooms)
    {
        foreach(RoomData room in rooms)
        {
            int centerX = Mathf.RoundToInt(room.Center.x);
            int centerY = Mathf.RoundToInt(room.Center.y);
            int halfWidth = room.Width / 2;
            int halfHeight = room.Height / 2;

            room.Center = new Vector2(centerX, centerY);
            room.Bounds = new RoomBounds
            {
                Left = centerX - halfWidth,
                Right = centerX + halfWidth,
                Bottom = centerY - halfHeight,
                Top = centerY + halfHeight
            };
        }
    }

    /**
     * @brief 후보 간선에서 최소 신장 트리를 생성하여 방들을 연결한다.
     * @param rooms 연결할 방 목록
     * @return 크루스칼 알고리즘으로 선택된 연결 간선 목록
     */
    public List<ConnectionEdge> LinkRoom(List<RoomData> rooms)
    {
        if (rooms == null)
        {
            throw new System.ArgumentNullException(nameof(rooms));
        }

        if (rooms.Count <= 1)
        {
            return new List<ConnectionEdge>();
        }

        List<List<Edge>> candidateEdges = getCandidateEdge(rooms, mNearestNeighborAmount);
        List<ConnectionEdge> connections = doKruskal(candidateEdges);

        // K-Nearest 후보 그래프가 분리된 경우 전체 간선으로 MST를 다시 생성한다.
        if (connections.Count != rooms.Count - 1)
        {
            candidateEdges = getCandidateEdge(rooms, rooms.Count - 1);
            connections = doKruskal(candidateEdges);
        }

        return connections;
    }

    /**
     * @brief 모든 연결 간선의 문 후보를 생성한다.
     * @param rooms 연결된 방 목록
     * @param connections 크루스칼 알고리즘으로 선택된 연결 간선 목록
     * @return 간선별 문 후보를 저장한 연결 계획 목록
     */
    public List<ConnectionPlan> CreateDoorCandidates( List<RoomData> rooms, List<ConnectionEdge> connections)
    {
        List<ConnectionPlan> connectionPlans = new List<ConnectionPlan>(connections.Count);

        foreach (ConnectionEdge connection in connections)
        {
            RoomData roomA = rooms[connection.mFrom];
            RoomData roomB = rooms[connection.mTo];
            connectionPlans.Add(createDoorCandidate(roomA, roomB));
        }

        return connectionPlans;
    }

    /**
     * @brief 문 후보 좌표를 복도 생성에 사용할 임시 타일 좌표로 변환한다.
     * @param connectionPlans 변환할 연결 계획 목록
     */
    public void CreateDoorAndEntryCells(List<RoomData> rooms, List<ConnectionPlan> connectionPlans)
    { 
        foreach (ConnectionPlan plan in connectionPlans)
        {
            RoomData fromRoom = rooms[plan.FromRoomID];
            RoomData toRoom = rooms[plan.ToRoomID];

            calculateDoorAndEntryCells(fromRoom, plan.FromDoorCandidate, plan.FromDoorSide, out Vector2Int fromDoorCell, out Vector2Int fromEntryCell);
            calculateDoorAndEntryCells(toRoom, plan.ToDoorCandidate, plan.ToDoorSide, out Vector2Int toDoorCell, out Vector2Int toEntryCell);

            plan.FromDoorCell = fromDoorCell;
            plan.FromEntryCell = fromEntryCell;
            plan.ToDoorCell = toDoorCell;
            plan.ToEntryCell = toEntryCell;
            plan.CorridorWaypoints.Clear();
            plan.CorridorPath.Clear();
        }
    }

    /**
     * @brief 각 문 후보가 방의 어느 벽에 위치하는지 판정한다.
     * @param rooms 연결된 방 목록
     * @param connectionPlans 판정할 연결 계획 목록
     */
    public void DetermineDoorSides(List<RoomData> rooms, List<ConnectionPlan> connectionPlans)
    {
        Debug.Assert(rooms != null);
        foreach (ConnectionPlan plan in connectionPlans)
        {
            RoomData fromRoom = rooms[plan.FromRoomID];
            RoomData toRoom = rooms[plan.ToRoomID];

            plan.FromDoorSide = calculateDoorSide(fromRoom, plan.FromDoorCandidate);
            plan.ToDoorSide = calculateDoorSide(toRoom, plan.ToDoorCandidate);
            plan.CorridorWaypoints.Clear();
            plan.CorridorPath.Clear();
        }
    }

    /**
     * @brief 같은 축의 문 쌍에 Z자 복도 경유점을 생성한다.
     * @param connectionPlans 경유점을 생성할 연결 계획 목록
     */
    public void CreateSameAxisCorridorWaypoints(List<ConnectionPlan> connectionPlans)
    {
        foreach (ConnectionPlan plan in connectionPlans)
        {
            bool fromHorizontal = isHorizontalDoorSide(plan.FromDoorSide);
            bool toHorizontal = isHorizontalDoorSide(plan.ToDoorSide);

            plan.CorridorWaypoints.Clear();
            plan.CorridorPath.Clear();

            // 두 문이 같은 축에 위치하지 않으면 Z자 복도 경유점을 생성하지 않는다
            if (fromHorizontal != toHorizontal) continue;


            // 시작점 추가
            addCorridorWaypoint(plan.CorridorWaypoints, plan.FromDoorCell);

            // 꺾임점 추가
            if (fromHorizontal)
            {
                int middleX = Mathf.RoundToInt((plan.FromDoorCell.x + plan.ToDoorCell.x) * 0.5f);

                addCorridorWaypoint(plan.CorridorWaypoints, new Vector2Int(middleX, plan.FromDoorCell.y));
                addCorridorWaypoint(plan.CorridorWaypoints, new Vector2Int(middleX, plan.ToDoorCell.y));
            }
            else
            {
                int middleY = Mathf.RoundToInt((plan.FromDoorCell.y + plan.ToDoorCell.y) * 0.5f);

                addCorridorWaypoint(plan.CorridorWaypoints, new Vector2Int(plan.FromDoorCell.x, middleY));
                addCorridorWaypoint(plan.CorridorWaypoints, new Vector2Int(plan.ToDoorCell.x, middleY));
            }

            // 도착점 추가
            addCorridorWaypoint(plan.CorridorWaypoints, plan.ToDoorCell);
        }
    }

    /**
     * @brief 복도 경유점 사이를 한 칸 간격의 정수 셀로 채운다
     * @param connectionPlans 복도 경로를 생성할 연결할 데이터
     */
    public void CreateCorridorPaths(List<ConnectionPlan> connectionPlans)
    {
        foreach (ConnectionPlan plan in connectionPlans)
        {
            plan.CorridorPath.Clear();

            for (int i = 0; i < plan.CorridorWaypoints.Count - 1; ++i)
            {
                addCorridorSegment(plan.CorridorPath, plan.CorridorWaypoints[i], plan.CorridorWaypoints[i + 1]);
            }
        }
    }


    // 전체 맵이 들어갈 크기를 확보하기 위한 배열
    public MapGridData CreateEmptyMapGridData(List<RoomData> rooms, List<ConnectionPlan> connectionPlans)
    {
        if (rooms.Count == 0)
        {
            return new MapGridData(Vector2Int.zero, 0, 0);
        }

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (RoomData room in rooms)
        {
            minX = System.Math.Min(minX, room.Bounds.Left);
            maxX = System.Math.Max(maxX, room.Bounds.Right);
            minY = System.Math.Min(minY, room.Bounds.Bottom);
            maxY = System.Math.Max(maxY, room.Bounds.Top);
        }

        if (connectionPlans != null)
        {
            foreach (ConnectionPlan plan in connectionPlans)
            {
                includeCell(plan.FromDoorCell, ref minX, ref maxX, ref minY, ref maxY);
                includeCell(plan.ToDoorCell, ref minX, ref maxX, ref minY, ref maxY);

                foreach (Vector2Int corridorCell in plan.CorridorPath)
                {
                    includeCell(corridorCell, ref minX, ref maxX, ref minY, ref maxY);
                }
            }
        }

        const int wallPadding = 1;
        minX -= wallPadding;
        maxX += wallPadding;
        minY -= wallPadding;
        maxY += wallPadding;

        return new MapGridData(new Vector2Int(minX, minY), maxX - minX + 1, maxY - minY + 1);
    }

    // 방 저장
    public void FillRoomCells(MapGridData mapGridData, List<RoomData> rooms)
    {
        if (mapGridData == null)
        {
            throw new System.ArgumentNullException(nameof(mapGridData));
        }

        if (rooms == null)
        {
            throw new System.ArgumentNullException(nameof(rooms));
        }

        foreach (RoomData room in rooms)
        {
            for (int cellY = room.Bounds.Bottom; cellY <= room.Bounds.Top; ++cellY)
            {
                for (int cellX = room.Bounds.Left; cellX <= room.Bounds.Right; ++cellX)
                {
                    setCell(mapGridData, new Vector2Int(cellX, cellY), MapCellType.Floor);
                }
            }
        }
    }

    // 복도,문 저장
    public void FillConnectionCells(MapGridData mapGridData, List<ConnectionPlan> connectionPlans)    
    {
        foreach (ConnectionPlan plan in connectionPlans)
        {
            foreach (Vector2Int corridorCell in plan.CorridorPath)
            {
                setCell(mapGridData, corridorCell, MapCellType.Floor);
            }

            setCell(mapGridData, plan.FromDoorCell, MapCellType.Floor);
            setCell(mapGridData, plan.ToDoorCell, MapCellType.Floor);
            mapGridData.DoorCells.Add(plan.FromDoorCell);
            mapGridData.DoorCells.Add(plan.ToDoorCell);
        }
    }

    public void CreateWallCells(MapGridData mapGridData)
    {
        if (mapGridData == null)
        {
            throw new System.ArgumentNullException(nameof(mapGridData));
        }

        for (int arrayY = 0; arrayY < mapGridData.Height; ++arrayY)
        {
            for (int arrayX = 0; arrayX < mapGridData.Width; ++arrayX)
            {
                if (mapGridData.Cells[arrayX, arrayY] != MapCellType.Floor)
                {
                    continue;
                }

                for (int offsetY = -1; offsetY <= 1; ++offsetY)
                {
                    for (int offsetX = -1; offsetX <= 1; ++offsetX)
                    {
                        int neighborX = arrayX + offsetX;
                        int neighborY = arrayY + offsetY;

                        if (neighborX < 0 || neighborX >= mapGridData.Width ||
                            neighborY < 0 || neighborY >= mapGridData.Height)
                        {
                            continue;
                        }

                        if (mapGridData.Cells[neighborX, neighborY] == MapCellType.Empty)
                        {
                            mapGridData.Cells[neighborX, neighborY] = MapCellType.Wall;
                        }
                    }
                }
            }
        }
    }

    /**************************************************************************/
    // Private Functions
    /**************************************************************************/

    /**
     * @brief 두 방을 AABB알고리즘을 통해 겹치는지 검사한다.
     * @detail
     * 각 방의 Width와 Height에 roomMargin을 더한 확장 영역을 기준으로 검사한다.
     * 
     * @param a 검사할 첫 번째 방
     * @param b 검사할 두 번째 방
     * @param roomMargin 방 크기에 추가로 반영할 여백
     * @return 두 방이 겹치면 true, 겹치지 않으면 false 반환
     */
    private bool aabb(RoomData a, RoomData b, float roomMargin)
    {
        float aMinX = a.Center.x - (a.Width + roomMargin) * 0.5f;
        float aMaxX = a.Center.x + (a.Width + roomMargin) * 0.5f;
        float aMinY = a.Center.y - (a.Height + roomMargin) * 0.5f;
        float aMaxY = a.Center.y + (a.Height + roomMargin) * 0.5f;

        float bMinX = b.Center.x - (b.Width + roomMargin) * 0.5f;
        float bMaxX = b.Center.x + (b.Width + roomMargin) * 0.5f;
        float bMinY = b.Center.y - (b.Height + roomMargin) * 0.5f;
        float bMaxY = b.Center.y + (b.Height + roomMargin) * 0.5f;

        return aMinX < bMaxX && aMaxX > bMinX && aMinY < bMaxY && aMaxY > bMinY;
    }


    /**
     * @brief K_Nearest이용하여 노드들을 근처 k개만큼의 노드와 연결하는 함수
     * @param rooms 후보 간선을 생성할 방 목록
     * @param nearestNeighborAmount 각 방에서 선택할 인접 방 개수
     * @return 2차 리스트로 구현한 인접리스트
     * O(n^2)이라 최적화 가능하면 하기
     * @warning 해당 방법은 방의 크기는 고려하지 않고 방의 중심만 고려하는 문제가 있음.
     * 또한 RoomID를 인덱스처럼 사용하고 있어 roomID가 연속적이어야 한다.
     * 그래프가 2개 이상이 생길 수 있음에 주의
     */
    private List<List<Edge>> getCandidateEdge(List<RoomData> rooms, int nearestNeighborAmount)
    {
        int roomCount = rooms.Count;
        List<List<Edge>> candidateEdges = new List<List<Edge>>(roomCount);
        for (int i = 0; i < roomCount; i++)
        {
            candidateEdges.Add(new List<Edge>());
        }

        // 모든 연결을 통한 인접 리스트 생성
        foreach (RoomData roomA in rooms)
        {
            foreach (RoomData roomB in rooms)
            {
                if (roomA.RoomID == roomB.RoomID) continue;

                Vector2 dirVec = (roomA.Center - roomB.Center);
                // 직각 이동이기 때문에, 맨해튼 거리 사용하여 비교
                float distance = Mathf.Abs(dirVec.x) + Mathf.Abs(dirVec.y);
                candidateEdges[roomA.RoomID].Add(new Edge(roomB.RoomID, distance));
            }
        }

        // 각 1차원 리스트를 오름차순으로 정렬 후 k개만 남기고 삭제
        foreach (List<Edge> edges in candidateEdges)
        {
            edges.Sort((l, r) => l.mDistance.CompareTo(r.mDistance));

            int neighborCount = nearestNeighborAmount;
            if (neighborCount < 0)
            {
                neighborCount = 0;
            }
            else if (neighborCount > edges.Count)
            {
                neighborCount = edges.Count;
            }

            edges.RemoveRange(neighborCount, edges.Count - neighborCount);
        }

        return candidateEdges;
    }


    /**
     * @brief 후보 간선 목록에 크루스칼 알고리즘을 적용한다.
     * @param edges 방별 후보 간선 인접 리스트
     * @return 사이클 없이 선택된 최소 비용 간선 목록
     */
    private List<ConnectionEdge> doKruskal(List<List<Edge>> edges)
    {
        int roomCount = edges.Count;
        List<ConnectionEdge> sortedEdges = new List<ConnectionEdge>();
        HashSet<long> edgeKeys = new HashSet<long>();

        // 인접 리스트를 간선 목록으로 변환하고 무방향 중복 간선을 제거한다.
        for (int from = 0; from < roomCount; ++from)
        {
            foreach (Edge edge in edges[from])
            {
                // 무방향 간선의 중복을 제거하기 위해 두 방 ID를 정렬하여 일부로 같게 만듬
                int minRoomId = from < edge.mRoomId ? from : edge.mRoomId;
                int maxRoomId = from < edge.mRoomId ? edge.mRoomId : from;
                long edgeKey = ((long)minRoomId << 32) | (uint)maxRoomId;

                if (edgeKeys.Add(edgeKey) == false)
                {
                    continue;
                }

                sortedEdges.Add(new ConnectionEdge(minRoomId, maxRoomId, edge.mDistance));
            }
        }

        sortedEdges.Sort((left, right) => left.mDistance.CompareTo(right.mDistance));

        UnionFind unionFind = new UnionFind(roomCount);
        List<ConnectionEdge> result = new List<ConnectionEdge>(roomCount - 1);

        foreach (ConnectionEdge edge in sortedEdges)
        {
            if (unionFind.Union(edge.mFrom, edge.mTo) == false)
            {
                continue;
            }

            result.Add(edge);

            if (result.Count == roomCount - 1)
            {
                break;
            }
        }

        return result;
    }


    /**
     * @brief 두 방을 연결하는 문 후보를 생성한다.
     * @param roomA 연결할 첫 번째 방
     * @param roomB 연결할 두 번째 방
     * @return 두 방과 각 방의 문 후보를 저장한 연결 계획
     */
    private ConnectionPlan createDoorCandidate(RoomData roomA, RoomData roomB)
    {
        Vector2 roomADoorCandidate = calculateDoorCandidate(roomA, roomB.Center);
        Vector2 roomBDoorCandidate = calculateDoorCandidate(roomB, roomA.Center);

        return new ConnectionPlan(
            roomA.RoomID,
            roomB.RoomID,
            roomADoorCandidate,
            roomBDoorCandidate);
    }

    /**
     * @brief room에서 target으로 향하는 벡터와 방의 테두리의 접점을 계산한다.
     * @param room 연결을 시작하는 방
     * @param target 연결이 끝날 방의 중앙 좌표
     */
    private Vector2 calculateDoorCandidate(RoomData room, Vector2 target)
    {
        Vector2 direction = target - room.Center;

        float halfWidth = room.Width * 0.5f;
        float halfHeight = room.Height * 0.5f;

        // 간선 방향으로 먼저 벽에 도달하기까지 필요한 값
        float tx = halfWidth / Mathf.Abs(direction.x);
        float ty = halfHeight / Mathf.Abs(direction.y);

        // 적은 값이 먼저 벽과 접하는 지점임
        float intersectionRatio = Mathf.Min(tx, ty);
        return room.Center + direction * intersectionRatio;
    }

    /**
     * @brief 문 후보가 위치한 방의 벽 방향을 계산한다.
     */
    private DoorSide calculateDoorSide(RoomData room, Vector2 doorCandidate)
    {
        Vector2 offset = doorCandidate - room.Center;

        // 방 크기로 정규화한 축 거리의 비율을 계산 (나눗셈을 없애기 위한 곱셉 적용)
        float horizontalRatio = Mathf.Abs(offset.x) * room.Height;
        float verticalRatio = Mathf.Abs(offset.y) * room.Width;

        // 벽까지의 비율은 항상 100%므로 당연하게도 더 큰 비율이 문이 위치한 벽임
        if (horizontalRatio >= verticalRatio)
        {
            return offset.x < 0f ? DoorSide.Left : DoorSide.Right;
        }

        return offset.y < 0f ? DoorSide.Bottom : DoorSide.Top;
    }

    private void calculateDoorAndEntryCells(
        RoomData room,
        Vector2 doorCandidate,
        DoorSide doorSide,
        out Vector2Int doorCell,
        out Vector2Int entryCell)
    {
        switch (doorSide)
        {
            case DoorSide.Left:
            {
                int cellY = Mathf.Clamp(
                    Mathf.RoundToInt(doorCandidate.y),
                    room.Bounds.Bottom,
                    room.Bounds.Top);
                entryCell = new Vector2Int(room.Bounds.Left, cellY);
                doorCell = new Vector2Int(room.Bounds.Left - 1, cellY);
                return;
            }

            case DoorSide.Right:
            {
                int cellY = Mathf.Clamp(
                    Mathf.RoundToInt(doorCandidate.y),
                    room.Bounds.Bottom,
                    room.Bounds.Top);
                entryCell = new Vector2Int(room.Bounds.Right, cellY);
                doorCell = new Vector2Int(room.Bounds.Right + 1, cellY);
                return;
            }

            case DoorSide.Bottom:
            {
                int cellX = Mathf.Clamp(
                    Mathf.RoundToInt(doorCandidate.x),
                    room.Bounds.Left,
                    room.Bounds.Right);
                entryCell = new Vector2Int(cellX, room.Bounds.Bottom);
                doorCell = new Vector2Int(cellX, room.Bounds.Bottom - 1);
                return;
            }

            case DoorSide.Top:
            {
                int cellX = Mathf.Clamp(
                    Mathf.RoundToInt(doorCandidate.x),
                    room.Bounds.Left,
                    room.Bounds.Right);
                entryCell = new Vector2Int(cellX, room.Bounds.Top);
                doorCell = new Vector2Int(cellX, room.Bounds.Top + 1);
                return;
            }

            default:
                throw new System.InvalidOperationException("Door side must be determined first.");
        }
    }

    private bool isHorizontalDoorSide(DoorSide doorSide)
    {
        return doorSide == DoorSide.Left || doorSide == DoorSide.Right;
    }


    /**
     * @brief 직전 경유점과 중복되지 않을 때만 새 경유점을 추가한다.
     * @param waypoints 복도 경유점 목록
     * @param waypoint 추가할 경유점
     */
    private void addCorridorWaypoint(List<Vector2Int> waypoints, Vector2Int waypoint)
    {
        if (waypoints.Count == 0 || waypoints[waypoints.Count - 1] != waypoint)
        {
            waypoints.Add(waypoint);
        }
    }

    /**
     * @brief 수평 또는 수직인 두 경유점 사이의 모든 셀을 복도 경로에 추가한다.
     */
    private void addCorridorSegment(List<Vector2Int> corridorPath, Vector2Int start, Vector2Int end)
    {
        if (corridorPath.Count == 0 || corridorPath[corridorPath.Count - 1] != start)
        {
            corridorPath.Add(start);
        }

        // 진행방향
        Vector2Int step = new Vector2Int(end.x.CompareTo(start.x), end.y.CompareTo(start.y));
        Vector2Int current = start;

        while (current != end)
        {
            current += step;
            corridorPath.Add(current);
        }
    }

    private void includeCell(Vector2Int cell, ref int minX, ref int maxX, ref int minY, ref int maxY)
    {
        minX = System.Math.Min(minX, cell.x);
        maxX = System.Math.Max(maxX, cell.x);
        minY = System.Math.Min(minY, cell.y);
        maxY = System.Math.Max(maxY, cell.y);
    }

    private void setCell(MapGridData mapGridData, Vector2Int cell, MapCellType cellType)
    {
        int arrayX = cell.x - mapGridData.Origin.x;
        int arrayY = cell.y - mapGridData.Origin.y;
        mapGridData.Cells[arrayX, arrayY] = cellType;
    }
}
