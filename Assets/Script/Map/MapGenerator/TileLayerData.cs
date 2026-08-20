using System;
using UnityEngine;

public enum TileLayerType
{
    Ground,
    UpperGround,
    Wall
}

// Scriptable Object에 저장하기 위해 Serializable 사용
[Serializable]
public class TileLayerData
{
    [SerializeField] private TileLayerType layerType;
    [SerializeField] private int[] tileIDs;

    public TileLayerType LayerType => layerType;
    public int[] TileIDs => tileIDs;

    public TileLayerData(TileLayerType layerType, int[] tileIDs)
    {
        this.layerType = layerType;
        this.tileIDs = tileIDs;
    }
}
