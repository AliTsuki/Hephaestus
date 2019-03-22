using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class ChunkGenerateSystem : JobComponentSystem
{
    private struct ChunkGenerateSystemJob : IJobProcessComponentData<ChunkPositionComponent, BlockArrayComponent, BiomeComponent, ChunkFlagsComponent>
    {
        public void Execute([ReadOnly] ref ChunkPositionComponent chunkPosComp, ref BlockArrayComponent blockComp, [ReadOnly] ref BiomeComponent biomeComp, ref ChunkFlagsComponent flagsComp)
        {
            if(!flagsComp.HasGenerated)
            {
                for(int x = 0; x < Bootstrap.ChunkSize; x++)
                {
                    for(int y = 0; y < Bootstrap.ChunkSize; y++)
                    {
                        for(int z = 0; z < Bootstrap.ChunkSize; z++)
                        {
                            int3 pos = new int3(x, y, z);
                            pos = CoordinateMath.BlockInternalChunkCoordsToWorldCoords(pos, chunkPosComp.ChunkPosition);
                            float noise = Noise.GetNoise(pos, biomeComp.Frequency, biomeComp.Lacunarity, biomeComp.OctaveCount, biomeComp.Persistence, Bootstrap.Seed, biomeComp.Height, biomeComp.HeightMultiplier, biomeComp.YMultiplier);
                            float caveNoise = Noise.GetNoiseCave(pos, biomeComp.FrequencyCave, biomeComp.LacunarityCave, biomeComp.OctaveCountCave, biomeComp.PersistenceCave, Bootstrap.Seed, biomeComp.HeightCave, biomeComp.HeightMultiplierCave, biomeComp.YMultiplierCave);
                            System.Random r = new System.Random();
                            int cutoffMargin = r.Next(-4, 4);
                            // Above Ground Generation
                            // Air Layer / Cave Generation
                            if(noise > biomeComp.Cutoff || caveNoise > biomeComp.CutoffCave)
                            {
                                blockComp.Blocks[x, y, z] = 0;
                            }
                            // Top layer
                            else if(noise < biomeComp.Cutoff && noise > biomeComp.CutoffTopLayer)
                            {
                                
                                if(cutoffMargin + pos.y > biomeComp.LayerTop5YStart)
                                {
                                    blockComp.Blocks[x, y, z] = biomeComp.LayerTop5Block;
                                }
                                else if(cutoffMargin + pos.y < biomeComp.LayerTop5YStart && cutoffMargin + pos.y > biomeComp.LayerTop4YStart)
                                {
                                    blockComp.Blocks[x, y, z] = biomeComp.LayerTop4Block;
                                }
                                else if(cutoffMargin + pos.y < biomeComp.LayerTop4YStart && cutoffMargin + pos.y > biomeComp.LayerTop3YStart)
                                {
                                    blockComp.Blocks[x, y, z] = biomeComp.LayerTop3Block;
                                }
                                else if(cutoffMargin + pos.y < biomeComp.LayerTop3YStart && cutoffMargin + pos.y > biomeComp.LayerTop2YStart)
                                {
                                    blockComp.Blocks[x, y, z] = biomeComp.LayerTop2Block;
                                }
                                else if(cutoffMargin + pos.y < biomeComp.LayerTop2YStart && cutoffMargin + pos.y > biomeComp.LayerTop1YStart)
                                {
                                    blockComp.Blocks[x, y, z] = biomeComp.LayerTop1Block;
                                }
                                else
                                {
                                    blockComp.Blocks[x, y, z] = biomeComp.LayerTop0Block;
                                }
                            }
                            // Secondary Layer
                            else if(noise < biomeComp.CutoffTopLayer && noise > biomeComp.Cutoff2ndLayer)
                            {
                                cutoffMargin = r.Next(-4, 4);
                                if(cutoffMargin + pos.y > 100)
                                {
                                    blockComp.Blocks[x, y, z] = 1;
                                }
                                else if(cutoffMargin + pos.y < 100 && cutoffMargin + pos.y > 62)
                                {
                                    blockComp.Blocks[x, y, z] = 2;
                                }
                                else if(cutoffMargin + pos.y < 62 && cutoffMargin + pos.y > 30)
                                {
                                    blockComp.Blocks[x, y, z] = 2;
                                }
                                else
                                {
                                    blockComp.Blocks[x, y, z] = 1;
                                }
                            }
                            // Inner Layer
                            else
                            {
                                blockComp.Blocks[x, y, z] = 1;
                            }
                        }
                    }
                }
                flagsComp.HasGenerated = true;
            }
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
