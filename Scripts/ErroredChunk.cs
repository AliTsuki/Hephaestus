using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for handling errored chunks, exceptions thrown when trying to update errored chunks
public class ErroredChunk : Chunk
{
    public ErroredChunk(int px, int pz, World world) : base(px, pz, world)
    {

    }

    public override void Start()
    {
        throw new System.Exception("Tried to use Start in ErroredChunk class");
    }

    public override void Update()
    {
        throw new System.Exception("Tried to use Update in ErroredChunk class");
    }

    public override void OnUnityUpdate()
    {
        throw new System.Exception("Tried to use OnUnityUpdate in ErroredChunk class");
    }

}
