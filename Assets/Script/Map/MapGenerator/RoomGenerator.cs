using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator
{
    // 방 랜덤으로 생성
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

    // 방 충돌하여 밀어내기
    // n^2 방식이있고, 누적방식이 있다.
    // 누적방식이 좀 더 효율적임
    public void ResolveOverlap(List<RoomData> rooms, float roomMargin, float pushStrength)
    {
        // 충돌이 없을때까지 반복
        while (ResolveOverlapStep(rooms, roomMargin, pushStrength) == true) { };
    }


    // 방 충돌하여 밀어내기
    // 충돌이 일어나면 true, 충돌이 없으면 false 반환
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

                // 겹친 양 계산
                float overlapX = (roomA.Width * 0.5f + roomB.Width * 0.5f + roomMargin - Mathf.Abs(roomA.Center.x - roomB.Center.x)) * pushStrength;
                float overlapY = (roomA.Height * 0.5f + roomB.Height * 0.5f + roomMargin - Mathf.Abs(roomA.Center.y - roomB.Center.y)) * pushStrength;

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

    // AABB 충돌 검사
    // 충돌하면 True반환, 충돌하지 않으면 False반환
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

}