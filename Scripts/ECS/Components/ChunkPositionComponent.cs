using Unity.Entities;
using Unity.Mathematics;

public struct ChunkPositionComponent : IComponentData
{
    public int3 ChunkPosition;
}
