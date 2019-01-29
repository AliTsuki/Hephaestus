using System;
using System.Collections.Generic;

using UnityEngine;

// Class for managing the Game functions
public class GameManager : MonoBehaviour
{
    // GameManager variables/objects
    private bool isPlayerLoaded = false;
    public GameObject StartCamera;
    public GameObject UITEXT;
    public GameObject Player;
    public Vector3 PlayerPos { get; private set; }
    public static GameManager Instance;
    private MainLoopable main;
    private readonly List<Delegate> _Delegates = new List<Delegate>();

    // Noise variables
    public float dx = 30f;
    public float dz = 30f;
    public float dy = 10f;
    public float my = 0.72f;
    public float mul = 0.06f;
    public float cutoff = 0.86f;
    public float dcutoffgrass = 1.04f;
    public float dcutoffdirt = 1.2f;
    // Underground noise variables
    public float UGdx = 10f;
    public float UGdz = 10f;
    public float UGdy = 10f;
    public float UGmul = 1.21f;
    public float UGcutoff = 0.63f;
    
    // Static noise variables
    public static float Sdx = 30f;
    public static float Sdz = 30f;
    public static float Sdy = 10f;
    public static float Smy = 0.72f;
    public static float Smul = 0.06f;
    public static float Scutoff = 0.86f;
    public static float Sdcutoffgrass = 1.04f;
    public static float Sdcutoffdirt = 1.2f;
    // Static underground noise variables
    public static float SUGdx = 10f;
    public static float SUGdz = 10f;
    public static float SUGdy = 10f;
    public static float SUGmul = 1.21f;
    public static float SUGcutoff = 0.63f;


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
        //Uncomment the following lines for DevChunk testing
        // in World.Start() for lines:  _LoadedChunk.Add(new Chunk...
        // change to:                   _LoadedChunk.Add(new DevChunk...
        // then move sliders in editor to test Noise values on chunk gen
        //Sdx = this.dx;
        //Sdz = this.dz;
        //Sdy = this.dy;
        //Smy = this.my;
        //Smul = this.mul;
        //Scutoff = this.cutoff;
        //Sdcutoffgrass = this.dcutoffgrass;
        //Sdcutoffdirt = this.dcutoffdirt;
        //SUGdx = this.UGdx;
        //SUGdz = this.UGdz;
        //SUGdy = this.UGdy;
        //SUGmul = this.UGmul;
        //SUGcutoff = this.UGcutoff;
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
    public void StartPlayer(Vector3 Pos)
    {
        Instance.RegisterDelegate(new Action(() =>
        {
            {
                Destroy(this.StartCamera);
                Destroy(this.UITEXT);
                this.Player.transform.position = new Vector3(Pos.x, Pos.y, Pos.z);
                this.Player.SetActive(true);
                this.PlayerPos = this.Player.transform.position;
            }
        }));
    }
}
