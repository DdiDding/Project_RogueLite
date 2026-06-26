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
        public int RoomId;
        public float Distance;

        public Edge(int roomId, float sqrDistance)
        {
            RoomId = roomId;
            SqrDistance = sqrDistance;
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
            room.Height = Random.Range(10, 30);
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
     * @return void
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


    // 방연결
    public void LinkRoom(List<RoomData> rooms)
    {
        // 우선 연결하고,
        List<List<Edge>> candidateEdges = getCandidateEdge(rooms, k);

        // 프림이나 크루스칼 사용아여 최적 그래프로 변환
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
     * @param k 연결할 주변 노드 개수
     * @return 2차 리스트로 구현한 인접리스트
     * @todo 방 개수에 관하여 k값의 유효성 확인
     * O(n^2)이라 최적화 가능하면 하기
     * @warning 해당 방법은 방의 크기는 고려하지 않고 방의 중심만 고려하는 문제가 있음.
     * 또한 RoomID를 인덱스처럼 사용하고 있어 roomID가 연속적이어야 한다.
     * 그래프가 2개 이상이 생길 수 있음에 주의
     */
    private List<List<Edge>> getCandidateEdge(List<RoomData> rooms)
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
                float distance = abs(dirVec.x) + abs(dirVec.y);
                candidateEdges[roomA.RoomID].Add(new Edge(roomB.RoomID, distance));
            }
        }

        // 각 1차원 리스트를 오름차순으로 정렬 후 k개만 남기고 삭제
        foreach (List<Edge> edges in candidateEdges)
        {
            edges.Sort((l, r) => l.Distance.CompareTo(r.Distance));
            edges.RemoveRange(mNearestNeighborAmount, edges.Count - mNearestNeighborAmount);
        }

        return candidateEdges;
    }
}