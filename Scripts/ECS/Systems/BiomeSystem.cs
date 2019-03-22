using SharpNoise.Modules;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class BiomeSystem : JobComponentSystem
{
    private struct BiomeSystemJob : IJobProcessComponentData<ChunkPositionComponent, BiomeComponent, ChunkFlagsComponent>
    {
        public void Execute([ReadOnly] ref ChunkPositionComponent pos, ref BiomeComponent biome, ref ChunkFlagsComponent flags)
        {
            if(!flags.BiomeAssigned)
            {
                // TODO: Insert Biome Creation Code
                flags.BiomeAssigned = true;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new BiomeSystemJob
        {
            //
        };
        return job.Schedule(this, inputDeps);
    }
}
