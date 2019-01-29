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
        if(this.HasGenerated && !this.hasRendered && this.hasDrawn)
        {
            base.OnUnityUpdate();
            this.HasGenerated = false;
            this.hasDrawn = false;
            this.hasRendered = false;
            this.Start();
        }
    }
}
