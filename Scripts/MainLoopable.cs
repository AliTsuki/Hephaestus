using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for Main Loop
public class MainLoopable : ILoopable
{
    private static MainLoopable _Instance;
    public static MainLoopable GetInstance() => _Instance;

    private readonly List<ILoopable> _RegisteredLoops = new List<ILoopable>();

    public static void Instantiate()
    {
        _Instance = new MainLoopable();
        //register
        Logger.Instantiate();
        World.Instantiate();
        BlockRegistry.RegisterBlocks();
    }

    public void RegisterLoops(ILoopable l) => this._RegisteredLoops.Add(l);

    public void DeRegisterLoops(ILoopable l) => this._RegisteredLoops.Remove(l);

    // Start is called before the first frame update
    public void Start()
    {
        foreach(ILoopable l in this._RegisteredLoops)
        {
            l.Start();
        }
    }

    // Update is called once per frame
    public void Update()
    {
        //Logger.Log("Updating");
        foreach (ILoopable l in this._RegisteredLoops)
        {
            l.Update();
        }
    }

    public void OnApplicationQuit()
    {
        foreach (ILoopable l in this._RegisteredLoops)
        {
            l.OnApplicationQuit();
        }
    }
}
