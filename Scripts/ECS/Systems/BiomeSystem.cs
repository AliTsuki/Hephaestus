using SharpNoise.Modules;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class BiomeSystem : JobComponentSystem
{
    private struct BiomeSystemJob : IJobProcessComponentData<ChunkPositionComponent, BiomeComponent, ChunkFlagsComponent>
    {
        public Perlin BMTemp;
        public Perlin BMHumid;
        public Perlin BMHeight;

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
            BMTemp = Bootstrap.BMTemp,
            BMHumid = Bootstrap.BMHumid,
            BMHeight = Bootstrap.BMHeight
        };
        return job.Schedule(this, inputDeps);
    }
}
