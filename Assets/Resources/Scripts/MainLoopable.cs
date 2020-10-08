using System.Collections.Generic;

namespace OLD
{
    // Class for Main Loop
    public class MainLoopable : ILoopable
    {
        //MainLoopable fields
        public static MainLoopable Instance { get; private set; }
        private readonly List<ILoopable> registeredLoops = new List<ILoopable>();

        // Instantiate MainLoopable, Logger, and World, also Register Blocks
        public static void Instantiate()
        {
            Instance = new MainLoopable();
            Logger.Instantiate();
            World.Instantiate();
            Block.Instantiate();
            BlockRegistry.RegisterBlocks();
        }

        // Start is called before the first frame update
        // Main Loopable Start: Start Registered Loops
        public void Start()
        {
            foreach(ILoopable l in this.registeredLoops)
            {
                l.Start();
            }
        }

        // Update is called once per frame
        // Main Loopable Update: Update Registered Loops
        public void Update()
        {
            foreach(ILoopable l in this.registeredLoops)
            {
                l.Update();
            }
        }

        // Fixed Update called on timer, more than one per Update on slow FPS, less than one per Update on fast FPS
        public void FixedUpdate()
        {
            //if(World.WorldInstance._LoadedChunks != null)
            //{
            //    for(int i = 0; i < World.WorldInstance._LoadedChunks.Count; i++)
            //    {
            //        World.WorldInstance._LoadedChunks[i].Tick();
            //    }
            //}
        }

        // Main Loopable On Application Quit
        public void OnApplicationQuit()
        {
            foreach(ILoopable l in this.registeredLoops)
            {
                l.OnApplicationQuit();
            }
        }

        // Register Loops
        public void RegisterLoops(ILoopable _l)
        {
            this.registeredLoops.Add(_l);
        }

        // Deregister Loops
        public void DeRegisterLoops(ILoopable _l)
        {
            this.registeredLoops.Remove(_l);
        }
    }
}
