using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class MapTilemapView : MonoBehaviour
{
    [SerializeField] private MapDataAsset mapDataAsset;

    [Header("Tile ID 1")]
    [SerializeField] private TileBase ground01;
    [SerializeField] private TileBase upperGround01;
    [SerializeField] private TileBase wall01;

    [SerializeField] private bool loadOnStart;

    private void Start()
    {
        if (loadOnStart)
        {
            LoadMap();
        }
    }

    public void LoadMap()
    {
        if (!validateMapData())
        {
            return;
        }

        foreach (TileLayerType layerType in Enum.GetValues(typeof(TileLayerType)))
        {
            getOrCreateTilemap(layerType).ClearAllTiles();
        }

        BoundsInt bounds = new BoundsInt(
            0,
            0,
            0,
            mapDataAsset.Width,
            mapDataAsset.Height,
            1);

        foreach (TileLayerData layerData in mapDataAsset.Layers)
        {
            TileBase[] tiles = createTileArray(layerData);
            if (tiles == null)
            {
                return;
            }

            getOrCreateTilemap(layerData.LayerType).SetTilesBlock(bounds, tiles);
        }
    }

    private bool validateMapData()
    {
        if (mapDataAsset == null)
        {
            Debug.LogWarning("MapDataAsset is not assigned.", this);
            return false;
        }

        if (mapDataAsset.Width <= 0 || mapDataAsset.Height <= 0)
        {
            Debug.LogError("MapDataAsset size must be greater than zero.", this);
            return false;
        }

        if (mapDataAsset.Layers == null)
        {
            Debug.LogError("MapDataAsset layers are missing.", this);
            return false;
        }

        int expectedCellCount = mapDataAsset.Width * mapDataAsset.Height;

        foreach (TileLayerData layerData in mapDataAsset.Layers)
        {
            if (layerData == null || layerData.TileIDs == null ||
                layerData.TileIDs.Length != expectedCellCount)
            {
                Debug.LogError(
                    $"Every tile layer must contain {expectedCellCount} tile IDs.",
                    this);
                return false;
            }
        }

        return true;
    }

    private TileBase[] createTileArray(TileLayerData layerData)
    {
        TileBase[] tiles = new TileBase[layerData.TileIDs.Length];

        for (int index = 0; index < layerData.TileIDs.Length; ++index)
        {
            int tileID = layerData.TileIDs[index];
            if (tileID == 0)
            {
                continue;
            }

            TileBase tile = getTile(layerData.LayerType, tileID);
            if (tile == null)
            {
                Debug.LogError(
                    $"Tile is missing. Layer: {layerData.LayerType}, ID: {tileID}",
                    this);
                return null;
            }

            tiles[index] = tile;
        }

        return tiles;
    }

    private TileBase getTile(TileLayerType layerType, int tileID)
    {
        if (tileID != 1)
        {
            return null;
        }

        switch (layerType)
        {
            case TileLayerType.Ground:
                return ground01;

            case TileLayerType.UpperGround:
                return upperGround01;

            case TileLayerType.Wall:
                return wall01;

            default:
                return null;
        }
    }

    private Tilemap getOrCreateTilemap(TileLayerType layerType)
    {
        string layerName = layerType.ToString();
        Transform layerTransform = transform.Find(layerName);

        GameObject layerObject;
        if (layerTransform == null)
        {
            layerObject = new GameObject(layerName);
            layerObject.transform.SetParent(transform, false);
        }
        else
        {
            layerObject = layerTransform.gameObject;
        }

        Tilemap tilemap = layerObject.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            tilemap = layerObject.AddComponent<Tilemap>();
        }

        TilemapRenderer tilemapRenderer = layerObject.GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            tilemapRenderer = layerObject.AddComponent<TilemapRenderer>();
        }

        tilemapRenderer.sortingOrder = getSortingOrder(layerType);

        return tilemap;
    }

    private int getSortingOrder(TileLayerType layerType)
    {
        switch (layerType)
        {
            case TileLayerType.Ground:
                return 0;

            case TileLayerType.Wall:
                return 1;

            case TileLayerType.UpperGround:
                return 2;

            default:
                return 0;
        }
    }
}
