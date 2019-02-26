using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class ChunkGenerateSystem : JobComponentSystem
{
    private struct ChunkGenerateSystemJob : IJobProcessComponentData<PositionComponent, BlockArrayComponent, BiomeComponent, ChunkFlagsComponent>
    {
        public int ChunkSize;

        public void Execute([ReadOnly] ref PositionComponent pos, ref BlockArrayComponent bac, [ReadOnly] ref BiomeComponent biome, ref ChunkFlagsComponent flags)
        {
            if(!flags.HasGenerated)
            {
                int cutoffMargin;
                System.Random r = new System.Random();
                for(int x = 0; x < Bootstrap.ChunkSize; x++)
                {
                    for(int y = 0; y < Bootstrap.ChunkSize; y++)
                    {
                        for(int z = 0; z < Bootstrap.ChunkSize; z++)
                        {
                            float perlin = this.GetNoise(pos.Position, biome);
                            float perlinCave = this.GetNoiseCave(pos.Position, biome);
                            // Above Ground Generation
                            // Air Layer
                            if(perlin > biome.AirAndLandIntersectionCutoff)
                            {
                                bac.Blocks[x, y, z] = 0;
                            }
                            // Top layer
                            else if(perlin < biome.AirAndLandIntersectionCutoff && perlin > biome.LandTopLayerCutoff)
                            {
                                cutoffMargin = r.Next(-4, 4);
                                if(cutoffMargin + pos.Position.y > 110)
                                {
                                    bac.Blocks[x, y, z] = 5;
                                }
                                else if(cutoffMargin + pos.Position.y < 110 && cutoffMargin + pos.Position.y > 100)
                                {
                                    bac.Blocks[x, y, z] = 1;
                                }
                                else if(cutoffMargin + pos.Position.y < 100 && cutoffMargin + pos.Position.y > 90)
                                {
                                    bac.Blocks[x, y, z] = 2;
                                }
                                else if(cutoffMargin + pos.Position.y < 90 && cutoffMargin + pos.Position.y > 62)
                                {
                                    bac.Blocks[x, y, z] = 3;
                                }
                                else if(cutoffMargin + pos.Position.y < 62 && cutoffMargin + pos.Position.y > 30)
                                {
                                    bac.Blocks[x, y, z] = 4;
                                }
                                else
                                {
                                    bac.Blocks[x, y, z] = 1;
                                }
                            }
                            // Secondary Layer
                            else if(perlin < biome.LandTopLayerCutoff && perlin > biome.Land2NDLayerCutoff)
                            {
                                cutoffMargin = r.Next(-4, 4);
                                if(cutoffMargin + pos.Position.y > 100)
                                {
                                    bac.Blocks[x, y, z] = 1;
                                }
                                else if(cutoffMargin + pos.Position.y < 100 && cutoffMargin + pos.Position.y > 62)
                                {
                                    bac.Blocks[x, y, z] = 2;
                                }
                                else if(cutoffMargin + pos.Position.y < 62 && cutoffMargin + pos.Position.y > 30)
                                {
                                    bac.Blocks[x, y, z] = 2;
                                }
                                else
                                {
                                    bac.Blocks[x, y, z] = 1;
                                }
                            }
                            // Inner Layer
                            else
                            {
                                bac.Blocks[x, y, z] = 1;
                            }
                            // Cave Generation
                            if(perlinCave > biome.CaveCutoff)
                            {
                                bac.Blocks[x, y, z] = 0;
                            }
                        }
                    }
                }
                flags.HasGenerated = true;
            }
        }

        // Get noise
        private float GetNoise(int3 pos, BiomeComponent biome)
        {
            return (float)biome.Perlin.GetValue(pos.x, pos.y, pos.z) + ((pos.y - (128 * 0.3f)) * biome.YMultiplier);
        }

        // Get noise for Cave Generation
        private float GetNoiseCave(int3 pos, BiomeComponent biome)
        {
            return (float)biome.Ridged.GetValue(pos.x, pos.y, pos.z) - (pos.y / (128 * 0.5f) * biome.CaveYMultiplier);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ChunkGenerateSystemJob
        {
            // Insert data from main thread here
        };
        return job.Schedule(this, inputDeps);
    }
}
