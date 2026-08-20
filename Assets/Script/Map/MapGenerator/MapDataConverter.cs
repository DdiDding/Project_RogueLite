using System;
using UnityEngine;

public static class MapDataConverter
{
    public static MapDataAsset CreateMapDataAsset(MapGridData mapGridData)
    {
        int width = mapGridData.Width;
        int height = mapGridData.Height;
        int cellCount = width * height;

        int[] groundTileIDs = new int[cellCount];
        int[] upperGroundTileIDs = new int[cellCount];
        int[] wallTileIDs = new int[cellCount];

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int index = x + y * width;

                switch (mapGridData.Cells[x, y])
                {
                    case MapCellType.Ground:
                        groundTileIDs[index] = 1;
                        break;

                    case MapCellType.Wall:
                        wallTileIDs[index] = 1;
                        break;
                }
            }
        }

        TileLayerData[] layers =
        {
            new TileLayerData(TileLayerType.Ground, groundTileIDs),
            new TileLayerData(TileLayerType.UpperGround, upperGroundTileIDs),
            new TileLayerData(TileLayerType.Wall, wallTileIDs)
        };

        MapDataAsset mapDataAsset = ScriptableObject.CreateInstance<MapDataAsset>();
        mapDataAsset.SetData(width, height, layers);

        return mapDataAsset;
    }
}
