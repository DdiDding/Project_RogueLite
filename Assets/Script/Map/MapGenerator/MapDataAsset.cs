using UnityEngine;

public class MapDataAsset : ScriptableObject
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private TileLayerData[] layers;

    public int Width => width;
    public int Height => height;
    public TileLayerData[] Layers => layers;

    public void SetData(int width, int height, TileLayerData[] layers)
    {
        this.width = width;
        this.height = height;
        this.layers = layers;
    }
}
