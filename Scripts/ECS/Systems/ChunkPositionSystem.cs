using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

public class ChunkPositionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Transform pt = GameObject.Find("Player").transform;
        int3 playerPos = new int3(pt.position);
        int3 chunkPos = new int3(CoordinateMath.WorldCoordsToChunkCoords(playerPos));
        for(int x = -Bootstrap.RenderDistance; x < Bootstrap.RenderDistance; x++)
        {
            for(int y = -Bootstrap.RenderDistance; y < Bootstrap.RenderDistance; y++)
            {
                for(int z = -Bootstrap.RenderDistance; z < Bootstrap.RenderDistance; z++)
                {
                    int3 pos = new int3(x, y, z);
                    chunkPos = chunkPos + pos;
                    using(var chunks = GetComponentGroup(typeof(ChunkPositionComponent)).ToEntityArray(Allocator.TempJob))
                    {
                        foreach(ChunkPositionComponent cp in GetEntities<ChunkPositionComponent>())
                        {
                            if(!cp.ChunkPosition.Equals(chunkPos))
                            {
                                cp.ChunkPosition = chunkPos;
                            }
                        }
                    }  
                }
            }
        }
    }
}
