using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// Class for managing the Game functions
public class GameManager : MonoBehaviour
{
    // Instantiate Delegates
    private List<Delegate> _Delegates = new List<Delegate>();

    // Create objects
    public Camera camera;
    public Text UITEXT;
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
    public void RegisterDelegate(Delegate d)
    {
        _Delegates.Add(d);
    }

    // Create player in world and destroy starting UI
    public void StartPlayer(Vector3 Pos)
    {
        GameObject.Destroy(camera);
        GameObject.Destroy(UITEXT);
        GameObject t = Transform.Instantiate(Resources.Load<GameObject>("Player"), Pos, Quaternion.identity) as GameObject;
        t.transform.position = Pos;
        Player = t.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        FileManager.RegisterFiles();
        instance = this;
        TextureAtlas._Instance.CreateAtlas();
        MainLoopable.Instantiate();
        main = MainLoopable.GetInstance();
        main.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(Player != null)
        {
            playerpos = Player.transform.position;
            IsPlayerLoaded = true;
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
        main.Update();
        foreach(Delegate d in new List<Delegate>(_Delegates))
        {
            d.DynamicInvoke();
            _Delegates.Remove(d);
        }
    }

    // On Application Quit
    void OnApplicationQuit()
    {
        main.OnApplicationQuit();
    }

    // Exit Game internal
    internal static void exitGame()
    {
        instance.exitGameInstance();
    }

    // Exit Game
    public void exitGameInstance()
    {
        OnApplicationQuit();
    }

    // Check if Player is loaded into world
    internal static bool PlayerLoaded()
    {
        return instance.IsPlayerLoaded;
    }
}
