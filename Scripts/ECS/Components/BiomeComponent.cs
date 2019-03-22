using Unity.Entities;

public struct BiomeComponent : IComponentData
{
    public float Frequency;
    public float Lacunarity;
    public int OctaveCount;
    public float Persistence;
    public int Seed;
    public int Height;
    public float HeightMultiplier;
    public float YMultiplier;
    public float FrequencyCave;
    public float LacunarityCave;
    public int OctaveCountCave;
    public float PersistenceCave;
    public int HeightCave;
    public float HeightMultiplierCave;
    public float YMultiplierCave;
    public float Cutoff;
    public float CutoffTopLayer;
    public float Cutoff2ndLayer;
    public float CutoffCave;
    public int LayerTop5YStart;
    public int LayerTop4YStart;
    public int LayerTop3YStart;
    public int LayerTop2YStart;
    public int LayerTop1YStart;
    public int LayerTop0YStart;
    public int LayerTop5Block;
    public int LayerTop4Block;
    public int LayerTop3Block;
    public int LayerTop2Block;
    public int LayerTop1Block;
    public int LayerTop0Block;
}
