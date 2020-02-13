using System;
using System.Collections.Generic;

using UnityEngine;

// Class for managing the Game functions
public class GameManager : MonoBehaviour
{
    // GameManager fields
    public bool IsDebug = false;
    private bool isPlayerLoaded = false;
    public GameObject StartCamera;
    public GameObject UITEXT;
    public GameObject Player;
    public Vector3 PlayerPos { get; private set; }
    public static GameManager Instance;
    private MainLoopable main;
    private readonly List<Delegate> delegates = new List<Delegate>();
    public static string WorldName = "DevWorld";
    public static int WorldSeed = 0;
    public static string Time;

    // Start is called before the first frame update
    // GameManager Start: Register Files, Create Texture Atlas
    private void Start()
    {
        this.GetTime();
        Instance = this;
        FileManager.RegisterFiles();
        TextureAtlas.Instance.CreateAtlas();
        MainLoopable.Instantiate();
        this.main = MainLoopable.Instance;
        this.main.Start();
    }

    // Update is called once per frame
    // GameManager Update: Set Player position if Player exists, Update MainLoopable Instance, Invoke Delegates and Remove them
    private void Update()
    {
        this.GetTime();
        if(this.Player.activeSelf)
        {
            this.PlayerPos = this.Player.transform.position;
            Instance.isPlayerLoaded = true;
        }
        try
        {
            this.main.Update();
        }
        catch(Exception e)
        {
            Debug.Log($@"{Time}: Can't update MainLoopable due to Exception: {e.ToString()}");
            Logger.Log($@"{Time}: Can't update MainLoopable due to Exception: ");
            Logger.Log(e);
        }
        for(int i = 0; i < this.delegates.Count; i++)
        {
            try
            {
                this.delegates[i].DynamicInvoke();
                this.delegates.Remove(this.delegates[i]);
            }
            catch(Exception e)
            {
                Debug.Log($@"{Time}: Can't Invoke Delegate due to Exception: {e.ToString()}");
                Logger.Log($@"{Time}: Can't Invoke Delegate due to Exception: ");
                Logger.Log(e);
            }
        }
    }

    // Fixed Update called on timer, more than one per Update on slow FPS, less than one per Update on fast FPS
    private void FixedUpdate()
    {
        this.main.FixedUpdate();
    }

    // GameManager On Application Quit
    private void OnApplicationQuit()
    {
        this.main.OnApplicationQuit();
    }

    // Register Delegates
    public void RegisterDelegate(Delegate _d)
    {
        this.delegates.Add(_d);
    }

    // Check if Player is loaded into world
    public static bool PlayerLoaded()
    {
        return Instance.isPlayerLoaded;
    }

    // Create player in world and destroy starting UI
    public void StartPlayer(Vector3 _pos, GameObject _go)
    {
        Instance.RegisterDelegate(new Action(() =>
        {
            if(_go.GetComponent<MeshCollider>() != null)
            {
                Debug.Log($@"{Time}: Placing player in world...");
                Logger.Log($@"{Time}: Placing player in world...");
                Destroy(this.StartCamera);
                Destroy(this.UITEXT);
                this.Player.transform.position = new Vector3(_pos.x, _pos.y, _pos.z);
                this.PlayerPos = this.Player.transform.position;
                this.Player.SetActive(true);
            }
            else
            {
                Debug.Log($@"{Time}: Can't place player...");
                Logger.Log($@"{Time}: Can't place player...");
            }
        }));
    }

    // Get time for logging
    private void GetTime()
    {
        Time = DateTime.Now.ToLongTimeString();
    }
}
