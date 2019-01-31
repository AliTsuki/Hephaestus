using System;
using System.Collections.Generic;

using UnityEngine;

// Class for managing the Game functions
public class GameManager : MonoBehaviour
{
    // GameManager variables/objects
    public bool IsDebug = false;
    private bool isPlayerLoaded = false;
    public GameObject StartCamera;
    public GameObject UITEXT;
    public GameObject Player;
    public Vector3 PlayerPos { get; private set; }
    public static GameManager Instance;
    private MainLoopable main;
    private readonly List<Delegate> _Delegates = new List<Delegate>();
    
    //// Noise variables
    //public float dx = 50f;
    //public float dz = 50f;
    //public float dy = 50f;
    //public float my = 0.5f;
    //public float mul = 0.07f;
    //public float ndx = 20f;
    //public float ndz = 20f;
    //public float ndy = 20f;
    //public float nmul = 0.02f;
    //public float offset = 500f;
    //public float cutoff = 0.8f;
    //public float dcutoffgrass = 1.04f;
    //public float dcutoffdirt = 1.2f;
    //public float dcutoffstone = 1.4f;

    //// Mountain Stone noise variables
    //public float mdx = 20f;
    //public float mdz = 20f;
    //public float mdy = 20f;
    //public float mmy = 0.5f;
    //public float mmul = 0.01f;
    //public float mndx = 100f;
    //public float mndz = 100f;
    //public float mndy = 20f;
    //public float mnmul = 0.09f;
    //public float moffset = 1000f;
    //public float mcutoff = 1f;

    //// Underground noise variables
    //public float cavedx = 100f;
    //public float cavedz = 100f;
    //public float cavedy = 10f;
    //public float cavemul = 0.04f;
    //public float cavendx = 35f;
    //public float cavendz = 35f;
    //public float cavendy = 10f;
    //public float cavenmul = 0.09f;
    //public float caveoffset = -500f;
    //public float cavecutoff = 0.04f;

    //// Tree noise variables
    //public float treedx = 5f;
    //public float treedz = 5f;
    //public float treemul = 0.6f;
    //public float treeoffset = -1000f;
    //public float dcutofftreemax = 0.166f;
    //public float dcutofftreemin = 0.165f;
    //public float treendx = 100f;
    //public float treendz = 100f;
    //public float treenmul = 0.4f;

    // Static noise variables
    public static float Sdx = 50f;
    public static float Sdz = 50f;
    public static float Sdy = 50f;
    public static float Smy = 0.5f;
    public static float Smul = 0.07f;
    public static float Sndx = 20f;
    public static float Sndz = 20f;
    public static float Sndy = 20f;
    public static float Snmul = 0.02f;
    public static float Soffset = 500f;
    public static float Scutoff = 0.8f;
    public static float Sdcutoffgrass = 1.04f;
    public static float Sdcutoffdirt = 1.2f;
    public static float Sdcutoffstone = 1.4f;

    // Static Mountain Stone noise variables
    public static float Smdx = 10f;
    public static float Smdz = 10f;
    public static float Smdy = 10f;
    public static float Smmy = 0.5f;
    public static float Smmul = 0.001f;
    public static float Smndx = 40f;
    public static float Smndz = 40f;
    public static float Smndy = 20f;
    public static float Smnmul = 0.09f;
    public static float Smoffset = 1000f;
    public static float Smcutoff = 1.4f;
    public static float SDirtMinCutoff = 0.8f;
    public static float SDirtMaxCutoff = 0.9f;

    // Static underground noise variables
    public static float Scavedx = 100f;
    public static float Scavedz = 100f;
    public static float Scavedy = 10f;
    public static float Scavemul = 0.04f;
    public static float Scavendx = 35f;
    public static float Scavendz = 35f;
    public static float Scavendy = 20f;
    public static float Scavenmul = 0.1f;
    public static float Scaveoffset = 500f;
    public static float Scavecutoff = 0.04f;

    // Static Tree noise variables
    public static float Streedx = 5f;
    public static float Streedz = 5f;
    public static float Streemul = 0.6f;
    public static float Streeoffset = -1000f;
    public static float Sdcutofftreemax = 0.166f;
    public static float Sdcutofftreemin = 0.165f;
    public static float Streendx = 100f;
    public static float Streendz = 100f;
    public static float Streenmul = 0.4f;

    // Start is called before the first frame update
    // GameManager Start: Register Files, Create Texture Atlas
    void Start()
    {
        Instance = this;
        FileManager.RegisterFiles();
        TextureAtlas.Instance.CreateAtlas();
        MainLoopable.Instantiate();
        this.main = MainLoopable.GetInstance();
        this.main.Start();
    }

    // Update is called once per frame
    // GameManager Update: Set Player position if Player exists, Update MainLoopable Instance, Invoke Delegates and Remove them
    void Update()
    {
        if(this.Player.activeSelf)
        {
            this.PlayerPos = this.Player.transform.position;
            Instance.isPlayerLoaded = true;
        }
        // The following lines for DevChunk testing
        // in World.Start() for lines:  _LoadedChunk.Add(new Chunk...
        // change to:                   _LoadedChunk.Add(new DevChunk...
        // then move sliders in editor to test Noise values on chunk gen
        //if(this.IsDebug)
        //{
        //    Sdx = this.dx;
        //    Sdz = this.dz;
        //    Sdy = this.dy;
        //    Smy = this.my;
        //    Smul = this.mul;
        //    Sndx = this.ndx;
        //    Sndz = this.ndz;
        //    Sndy = this.ndy;
        //    Snmul = this.nmul;
        //    Soffset = this.offset;
        //    Scutoff = this.cutoff;
        //    Sdcutoffgrass = this.dcutoffgrass;
        //    Sdcutoffdirt = this.dcutoffdirt;
        //    Sdcutoffstone = this.dcutoffstone;
        //    Sdcutofftreemax = this.dcutofftreemax;
        //    Sdcutofftreemin = this.dcutofftreemin;
        //    Smdx = this.mdx;
        //    Smdz = this.mdz;
        //    Smdy = this.mdy;
        //    Smmul = this.mmul;
        //    Smmul = this.mmul;
        //    Smndx = this.mndx;
        //    Smndz = this.mndz;
        //    Smndy = this.mndy;
        //    Smnmul = this.mnmul;
        //    Smoffset = this.moffset;
        //    Smcutoff = this.mcutoff;
        //    Scavedx = this.cavedx;
        //    Scavedz = this.cavedz;
        //    Scavedy = this.cavedy;
        //    Scavemul = this.cavemul;
        //    Scavendx = this.cavendx;
        //    Scavendz = this.cavendz;
        //    Scavendy = this.cavendy;
        //    Scavenmul = this.cavenmul;
        //    Scaveoffset = this.caveoffset;
        //    Scavecutoff = this.cavecutoff;
        //}
        try
        {
            this.main.Update();
        }
        catch(System.Exception e)
        {
            Debug.Log("Can't update MainLoopable due to Exception.");
            Debug.Log(e.ToString());
            World.Instance.IsRunning = false;
        }
        for(int i = 0; i < this._Delegates.Count; i++)
        {
            this._Delegates[i].DynamicInvoke();
            this._Delegates.Remove(this._Delegates[i]);
        }
    }

    // GameManager On Application Quit
    void OnApplicationQuit()
    {
        this.main.OnApplicationQuit();
    }

    // Exit Game internal
    internal static void ExitGame()
    {
        Instance.ExitGameInstance();
    }

    // Exit Game method
    public void ExitGameInstance()
    {
        this.OnApplicationQuit();
    }

    // Register Delegates
    public void RegisterDelegate(Delegate d)
    {
        this._Delegates.Add(d);
    }

    // Check if Player is loaded into world
    internal static bool PlayerLoaded()
    {
        return Instance.isPlayerLoaded;
    }

    // Create player in world and destroy starting UI
    public void StartPlayer(Vector3 Pos, GameObject go)
    {
        Instance.RegisterDelegate(new Action(() =>
        {
            {
                if(go.gameObject.GetComponent<MeshCollider>().sharedMesh != null)
                {
                    Destroy(this.StartCamera);
                    Destroy(this.UITEXT);
                    this.Player.transform.position = new Vector3(Pos.x, Pos.y, Pos.z);
                    this.Player.SetActive(true);
                    this.PlayerPos = this.Player.transform.position;
                }
            }
        }));
    }
}
