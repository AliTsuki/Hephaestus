using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World variables/objects
    private bool IsRunning;
    private bool RanOnce = false;
    // How far to render chunks from Playerpos
    private static readonly int RenderDistanceChunks = 3;
    private Thread worldthread;
    private static Int3 Playerpos;
    private readonly List<Chunk> _LoadedChunks = new List<Chunk>();

    // World instance getter/setter
    public static World _instance { get; private set; }

    // Instantiate World, Register loops, set Random Player start position
    public static void Instantiate()
    {
        Debug.Log("World.Instantiate() executing...");
        _instance = new World();
        MainLoopable.GetInstance().RegisterLoops(_instance);
        System.Random r = new System.Random();
        Playerpos = new Int3(r.Next(-1000, 1000), 100, r.Next(-1000, 1000));
        Debug.Log("Starting Playerpos in WORLD set to: " + Playerpos.ToString());
    }

    // Start is called before the first frame update
    // Start world thread and generate Chunks in world thread
    public void Start()
    {
        this.IsRunning = true;
        this.worldthread = new Thread(() =>
        {
            Logger.Log("Initalizing world thread...");
            while(this.IsRunning)
            {
                // TODO: ERROR IN CHUNK CREATION/DEGENERATION IS IN THIS LOOP SOMEWHERE!! HAPPENS AFTER RANONCE LOOP
                try
                {
                    if(!this.RanOnce)
                    {
                        this.RanOnce = true;
                        // For first time running world thread, for all Chunk Positions within Rendering Distance 
                        // check if chunk exists in file, if so get from file, if not Generate Chunk
                        for(int x = -RenderDistanceChunks; x < RenderDistanceChunks; x++)
                        {
                            for(int z = -RenderDistanceChunks; z < RenderDistanceChunks; z++)
                            {
                                Int3 newchunkpos = new Int3(Playerpos.x, 0, Playerpos.z);
                                newchunkpos.AddPos(new Int3(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth));
                                newchunkpos.ToChunkCoordinates();
                                Debug.Log("During First Run: newchunkpos in Chunk Coords is: " + newchunkpos.ToString());
                                // If file exists for Chunk, read chunk data from file and add Chunk to _LoadedChunks
                                if (System.IO.File.Exists(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)))
                                {
                                    try
                                    {
                                        this._LoadedChunks.Add(new Chunk(newchunkpos.x, newchunkpos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)), this));
                                    }
                                    catch(System.Exception e)
                                    {
                                        Debug.Log(e.ToString());
                                    }
                                }
                                else
                                {
                                    this._LoadedChunks.Add(new Chunk(newchunkpos.x, newchunkpos.z, this));
                                    Debug.Log("During First Run: Can't find FILE for CHUNK: " + "C_" + newchunkpos.x + "_" + newchunkpos.z + ".CHK -> Creating CHUNK now...");
                                }
                            }
                        }
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            this._LoadedChunks[i].Start();
                        }
                        Debug.Log("World.RanOnce in World.Start()");
                    }
                    // After ran once, keep going
                    if(GameManager.PlayerLoaded())
                    {
                        Playerpos = new Int3(GameManager.instance.playerpos);
                        Debug.Log("NOT FIRST RUN: Playerpos = " + Playerpos);
                    }
                    for(int i = 0; i < this._LoadedChunks.Count; i++)
                    {
                        if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(Playerpos.x, Playerpos.z)) > (RenderDistanceChunks * 2 * Chunk.ChunkWidth))
                        {
                            this._LoadedChunks[i].Degenerate();
                        }
                    }
                    for(int x = -RenderDistanceChunks; x < RenderDistanceChunks; x++)
                    {
                        for(int z = -RenderDistanceChunks; z < RenderDistanceChunks; z++)
                        {
                            Int3 newchunkpos = new Int3(Playerpos.x, 0, Playerpos.z);
                            newchunkpos.AddPos(new Int3(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth));
                            newchunkpos.ToChunkCoordinates();
                            Debug.Log("NOT FIRST RUN: newchunkpos in Chunk Coords is: " + newchunkpos.ToString());
                            if(!this.ChunkExists(newchunkpos.x, newchunkpos.z))
                            {
                                if(System.IO.File.Exists(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)))
                                {
                                    try
                                    {
                                        Chunk c = new Chunk(newchunkpos.x, newchunkpos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)), this);
                                        c.Start();
                                        this._LoadedChunks.Add(c);
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.Log(e.ToString());
                                    }
                                }
                                else
                                {
                                    Chunk c = new Chunk(newchunkpos.x, newchunkpos.z, this);
                                    c.Start();
                                    this._LoadedChunks.Add(c);
                                    Debug.Log("NOT FIRST RUN: Can't find FILE for CHUNK: " + "C_" + newchunkpos.x + "_" + newchunkpos.z + ".CHK -> Creating CHUNK now.");
                                }
                            }
                        }
                    }
                    for(int i = 0; i < this._LoadedChunks.Count; i++)
                    {
                        this._LoadedChunks[i].Update();
                    }
                }
                catch(System.Exception e)
                {
                    Debug.Log(e.ToString());
                    Logger.Log(e);
                }
            }
            Logger.Log("World thread successfully stopped.");
            Logger.MainLog.Update(); // TODO: FIX IN FUTURE, BAD PRACTICE, This reruns last log
        });
        this.worldthread.Start();
    }

    // Remove Chunk from world
    internal void RemoveChunk(Chunk chunk)
    {
        this._LoadedChunks.Remove(chunk);
    }

    // Check if Chunk currently exists
    public bool ChunkExists(int posx, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return true;
            }
        }
        return false;
    }

    // Get Chunk at location
    public Chunk GetChunk(int posx, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return this._LoadedChunks[i];
            }
        }
        return new ErroredChunk(0, 0, this);
    }

    // Update is called once per frame
    // Update Chunks
    public void Update()
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            this._LoadedChunks[i].OnUnityUpdate();
        }
    }

    // On Application Quit, save Chunks to file and stop world thread
    public void OnApplicationQuit()
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            try
            {
                Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosZ), this._LoadedChunks[i].GetChunkSaveData());
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        this.IsRunning = false;
        Logger.Log("Stopping world thread...");
    }
}
