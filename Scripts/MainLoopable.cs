using System.Collections.Generic;

// Class for Main Loop
public class MainLoopable : ILoopable
{
    //MainLoopable objects
    public static MainLoopable MLInstance { get; private set; }
    private readonly List<ILoopable> _RegisteredLoops = new List<ILoopable>();

    // Instantiate MainLoopable, Logger, and World, also Register Blocks
    public static void Instantiate()
    {
        MLInstance = new MainLoopable();
        //register
        Logger.Instantiate();
        World.Instantiate();
        Block.Instantiate();
        BlockRegistry.RegisterBlocks();
    }

    // Start is called before the first frame update
    // Main Loopable Start: Start Registered Loops
    public void Start()
    {
        foreach(ILoopable l in this._RegisteredLoops)
        {
            l.Start();
        }
    }

    // Update is called once per frame
    // Main Loopable Update: Update Registered Loops
    public void Update()
    {
        foreach(ILoopable l in this._RegisteredLoops)
        {
            l.Update();
        }
    }

    // Main Loopable On Application Quit
    public void OnApplicationQuit()
    {
        foreach(ILoopable l in this._RegisteredLoops)
        {
            l.OnApplicationQuit();
        }
    }

    // Register Loops
    public void RegisterLoops(ILoopable l)
    {
        this._RegisteredLoops.Add(l);
    }

    // Deregister Loops
    public void DeRegisterLoops(ILoopable l)
    {
        this._RegisteredLoops.Remove(l);
    }
}
