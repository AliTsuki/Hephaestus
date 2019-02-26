using SharpNoise.Modules;

using Unity.Entities;

public struct BiomeComponent : IComponentData
{
    public Perlin Perlin;
    public RidgedMulti Ridged;
    public float YMultiplier;
    public float CaveYMultiplier;
    public float AirAndLandIntersectionCutoff;
    public float LandTopLayerCutoff;
    public float Land2NDLayerCutoff;
    public float CaveCutoff;
    public int Layer5YStart;
    public int Layer4YStart;
    public int Layer3YStart;
    public int Layer2YStart;
    public int Layer1YStart;
    public int Layer0YStart;
    public int Layer5Block;
    public int Layer4Block;
    public int Layer3Block;
    public int Layer2Block;
    public int Layer1Block;
    public int Layer0Block;
}
