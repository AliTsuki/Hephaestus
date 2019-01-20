using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for testing noise values of chunks
public class DevChunk : Chunk
{
    public DevChunk(int px, int pz, World world) : base(px, pz, world)
    {

    }

    public override void OnUnityUpdate()
    {
        if(HasGenerated && !HasRendered && HasDrawn)
        {
            base.OnUnityUpdate();

            HasGenerated = false;
            HasDrawn = false;
            HasRendered = false;

            Start();
        }
    }
}
