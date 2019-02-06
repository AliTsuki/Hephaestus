// Class for handling errored chunks, exceptions thrown when trying to update errored chunks
public class ErroredChunk : Chunk
{
    // ErroredChunk constructor
    public ErroredChunk(int px, int py, int pz) : base(px, py, pz)
    {

    }

    // ErroredChunk Start: throw exception
    public override void Start()
    {
        throw new System.Exception("Tried to use Start in ErroredChunk class");
    }

    // ErroredChunk Update: throw exception
    public override void Update()
    {
        throw new System.Exception("Tried to use Update in ErroredChunk class");
    }

    // ErroredChunk On Unity Update: throw exception
    public override void OnUnityUpdate()
    {
        throw new System.Exception("Tried to use OnUnityUpdate in ErroredChunk class");
    }
}
