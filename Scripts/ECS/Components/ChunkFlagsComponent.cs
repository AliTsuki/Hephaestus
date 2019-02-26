using Unity.Entities;

public struct ChunkFlagsComponent : IComponentData
{
    public BlitBool BiomeAssigned;
    public BlitBool HasGenerated;
    public BlitBool HasMeshed;
    public BlitBool HasRendered;
    public BlitBool NeedsUpdate;
    public BlitBool HasBeenModified;
}
