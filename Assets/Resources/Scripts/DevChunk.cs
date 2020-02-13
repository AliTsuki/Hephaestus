// Class for testing noise values of chunks
public class DevChunk : Chunk
{
    // DevChunk constructor
    public DevChunk(Int3 pos) : base(pos)
    {

    }

    // DevChunk On Unity Update: Set HasGenerated, HasDrawn, and HasRendered to false so chunks continuously update to reflect noise changes
    public override void OnUnityUpdate()
    {
        if(this.hasGenerated && !this.hasRendered && this.hasDrawn)
        {
            base.OnUnityUpdate();
            this.hasGenerated = false;
            this.hasDrawn = false;
            this.hasRendered = false;
            this.Start();
        }
    }
}
