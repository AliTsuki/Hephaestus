using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// Class for managing the Game functions
public class GameManager : MonoBehaviour
{
    // Instantiate Delegates
    private readonly List<Delegate> _Delegates = new List<Delegate>();

    // Create objects
    public Camera StartCamera;
    public Text UITEXT;
    public GameObject PlayerObject;
    public Transform Player;
    public Vector3 playerpos;

    // Noise variables
    public float dx = 1;
    public float dz = 1;
    public float my = 1;
    public float cutoff = 1;
    public float mul = 1;
    // Static noise variables
    public static float Sdx = 50;
    public static float Sdz = 50;
    public static float Smy = 0.23f;
    public static float Scutoff = 1.8f;
    public static float Smul = 1;

    // Is player loaded in world bool
    private bool IsPlayerLoaded = false;

    // Main objects
    public static GameManager instance;
    private MainLoopable main;

    // Register Delegates
    public void RegisterDelegate(Delegate d) => this._Delegates.Add(d);

    // Create player in world and destroy starting UI
    public void StartPlayer(Vector3 Pos)
    {
        Debug.Log("Running StartPlayer Method from GameManager");
        Destroy(this.StartCamera);
        Destroy(this.UITEXT);
        GameObject PlayerObject = Transform.Instantiate(Resources.Load<GameObject>("Player"), Pos, Quaternion.identity) as GameObject;
        PlayerObject.transform.position = Pos;
        this.Player = PlayerObject.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        FileManager.RegisterFiles();
        instance = this;
        TextureAtlas._Instance.CreateAtlas();
        MainLoopable.Instantiate();
        this.main = MainLoopable.GetInstance();
        this.main.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.Player != null)
        {
            this.playerpos = this.Player.transform.position;
            this.IsPlayerLoaded = true;
        }
        // Uncomment the following lines for DevChunk testing
        // in World.Start() for lines   _LoadedChunk.Add(new Chunk...
        // change to                    _LoadedChunk.Add(new DevChunk...
        // then move sliders in editor to test Noise values on chunk gen
        /*
        Sdx = dx;
        Sdz = dz;
        Smy = my;
        Scutoff = cutoff;
        Smul = mul;
        */
        this.main.Update();
        foreach(Delegate d in new List<Delegate>(this._Delegates))
        {
            d.DynamicInvoke();
            this._Delegates.Remove(d);
        }
    }

    // On Application Quit
    void OnApplicationQuit() => this.main.OnApplicationQuit();

    // Exit Game internal
    internal static void ExitGame() => instance.ExitGameInstance();

    // Exit Game
    public void ExitGameInstance() => this.OnApplicationQuit();

    // Check if Player is loaded into world
    internal static bool PlayerLoaded() => instance.IsPlayerLoaded;
}
