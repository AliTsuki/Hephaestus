using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for testing noise values of chunks
public class DevChunk : Chunk
{
    // DevChunk constructor
    public DevChunk(int px, int pz, World world) : base(px, pz, world)
    {

    }

    // DevChunk On Unity Update: Set HasGenerated, HasDrawn, and HasRendered to false so chunks continuously update to reflect noise changes
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
