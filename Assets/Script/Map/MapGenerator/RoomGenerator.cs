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
            room.Width = Random.Range(10, 30);

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
            room.Center = new Vector2(Mathf.RoundToInt(room.Center.x), Mathf.RoundToInt(room.Center.y));
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
     * 문 만들기
     */
    private void CreateDoor(RoomData roomA, RoomData roomB)
    {
        // 차이가 큰 축을 기준으로 연결 방향 선택
        Vector2 delta = roomA.Center - roomB.Center;


        // 마주 보는 벽 선택

        // 벽 범위 양끝에서 제한하여 모서리에 문이 생기지 않도록 제한

        // 
    }

}
