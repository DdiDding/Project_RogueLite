using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class RoomGenerator
{
    public enum PrimitiveType
    {
        None,
        Rectangle,
        Circle,
        Triangle,
        Max
    }
    public void GenerateRoom()
    {
        // Room generation logic goes here
    }


    ////////////////////////////////////////////
    // private Function
    // 도형을 겹쳐 방을 생성하는 함수
    private Room doPrimitiveComposition()
    {
        Room result;

       
    }

    // 랜덤한 사각형의 좌표를
    private List<Vector2> calculateRandomSquareCells()
    {
        var random = new System.Random();
        int value = random.Next(
        List<Vector2> result;
    }
}
