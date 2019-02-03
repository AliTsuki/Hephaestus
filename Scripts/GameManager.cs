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
    public static string WorldName = "DevWorld";

    // New Noise Variables
    public static float STATICyMultiplier = 0.04f;
    public static float STATICLand2NDLayerCutoff = 0.6f;
    public static float STATICLandTopLayerCutoff = 0.9f;
    public static float STATICAirAndLandIntersectionCutoff = 1.4f;
    public static float STATICPerlinFrequency = 0.015f;
    public static float STATICPerlinLacunarity = 2f;
    public static int STATICPerlinOctaveCount = 4;
    public static float STATICPerlinPersistance = 0.4f;
    public static int STATICPerlinSeed = 0;
    // New Cave Noise Variables
    public static float STATICCaveyMultiplier = 0.3f;
    public static float STATICCaveCutoff = 0.6f;
    public static float STATICRidgedFrequency = 0.03f;
    public static float STATICRidgedLacunarity = 2f;
    public static int STATICRidgedOctaveCount = 4;
    public static int STATICRidgedSeed = 0;
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
        this.main = MainLoopable.MLInstance;
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
        try
        {
            this.main.Update();
        }
        catch(System.Exception e)
        {
            Debug.Log("Can't update MainLoopable due to Exception.");
            Debug.Log(e.ToString());
            World.WorldInstance.IsRunning = false;
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
