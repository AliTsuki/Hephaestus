using SharpNoise.Modules;

using Unity.Entities;

using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public static int ChunkSize = 16;
    public static int Seed = 0;
    public static Perlin BMTemp = new Perlin
    {
        Frequency = 0.005,
        Lacunarity = 1,
        OctaveCount = 2,
        Persistence = 2,
        Seed = Seed
    };
    public static Perlin BMHumid = new Perlin
    {
        Frequency = 0.005,
        Lacunarity = 1,
        OctaveCount = 2,
        Persistence = 2,
        Seed = Seed + 1
    };
    public static Perlin BMHeight = new Perlin
    {
        Frequency = 0.005,
        Lacunarity = 1,
        OctaveCount = 2,
        Persistence = 2,
        Seed = Seed + 2
    };

    private void Start()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        var chunkEntity = entityManager.CreateArchetype(
            ComponentType.Create<PositionComponent>(),
            ComponentType.Create<ChunkPositionComponent>(),
            ComponentType.Create<MeshComponent>(),
            ComponentType.Create<BlockArrayComponent>(),
            ComponentType.Create<BiomeComponent>(),
            ComponentType.Create<ChunkFlagsComponent>()
        );
    }
}
