using Unity.Entities;

public struct BlockArrayComponent : IComponentData
{
    public int[,,] Blocks;
}
