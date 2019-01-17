using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction
{
    public bool InteractWithBlocks(Transform cam)
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(cam.position, cam.forward);

            if(Physics.Raycast(ray, out RaycastHit hitData))
            {
                if(World.instance.GetChunkAt(hitData.point, hitData.normal, false, out Chunk chunk))
                {
                    return chunk.SetBlockAt(hitData.point, hitData.normal, Block.Air);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = new Ray(cam.position, cam.forward);

            if (Physics.Raycast(ray, out RaycastHit hitData))
            {
                if (World.instance.GetChunkAt(hitData.point, hitData.normal, true, out Chunk chunk))
                {
                    return chunk.SetBlockAt(hitData.point, hitData.normal, Block.Stone, true);
                }
            }
        }
        return false;
    }
}
