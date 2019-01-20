using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    public static World _instance { get; private set; }
    private bool IsRunning;
    private Thread worldthread;
    private static Int3 Playerpos;
    private static readonly int RenderDistanceChunks = 3;

    public static void Instantiate()
    {
        _instance = new World();
        MainLoopable.GetInstance().RegisterLoops(_instance);
        System.Random r = new System.Random();
        Playerpos = new Int3(r.Next(-1000,1000),100, r.Next(-1000, 1000));
    }

    private bool RanOnce = false;
    private List<Chunk> _LoadedChunks = new List<Chunk>();

    // Start is called before the first frame update
    public void Start()
    {
        IsRunning = true;
        worldthread = new Thread(() =>
        {
            Logger.Log("Initalizing world thread");
            while(IsRunning)
            {
                try
                {
                    if(!RanOnce)
                    {
                        RanOnce = true;
                        for(int x = -RenderDistanceChunks; x < RenderDistanceChunks; x++)
                        {
                            for(int z = -RenderDistanceChunks; z < RenderDistanceChunks; z++)
                            {
                                Int3 newchunkpos = new Int3(Playerpos.x, Playerpos.y, Playerpos.z);
                                newchunkpos.AddPos(new Int3(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth));
                                newchunkpos.ToChunkCoordinates();

                                if (System.IO.File.Exists(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)))
                                {
                                    try
                                    {
                                        _LoadedChunks.Add(new Chunk(newchunkpos.x, newchunkpos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)), this));
                                    }
                                    catch(System.Exception e)
                                    {
                                        Debug.Log(e.ToString());
                                    }
                                }
                                else
                                {
                                    _LoadedChunks.Add(new Chunk(newchunkpos.x, newchunkpos.z, this));
                                    Debug.Log("Can't find: " + "Data/C" + x + "_" + z + ".CHK");
                                }
                            }
                        }
                        foreach(Chunk c in _LoadedChunks)
                        {
                            c.Start();
                        }
                    }
                    if(GameManager.PlayerLoaded())
                    {
                        Playerpos = new Int3(GameManager.instance.playerpos);
                    }
                    foreach(Chunk c in _LoadedChunks)
                    {
                        if(Vector2.Distance(new Vector2(c.PosX * Chunk.ChunkWidth, c.PosZ * Chunk.ChunkWidth), new Vector2(Playerpos.x, Playerpos.z)) > ((RenderDistanceChunks * 2) * Chunk.ChunkWidth))
                        {
                            c.Degenerate();
                        }
                    }

                    for (int x = -RenderDistanceChunks; x < RenderDistanceChunks; x++)
                    {
                        for (int z = -RenderDistanceChunks; z < RenderDistanceChunks; z++)
                        {
                            Int3 newchunkpos = new Int3(Playerpos.x, Playerpos.y, Playerpos.z);
                            newchunkpos.AddPos(new Int3(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth));
                            newchunkpos.ToChunkCoordinates();
                            if(!ChunkExists(newchunkpos.x, newchunkpos.z))
                            {
                                if (System.IO.File.Exists(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)))
                                {
                                    try
                                    {
                                        Chunk c = new Chunk(newchunkpos.x, newchunkpos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)), this);
                                        c.Start();
                                        _LoadedChunks.Add(c);
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
                                    _LoadedChunks.Add(c);
                                    Debug.Log("Can't find: " + "Data/C" + x + "_" + z + ".CHK");
                                }
                            }
                        }
                    }

                    foreach (Chunk c in new List<Chunk>(_LoadedChunks))
                    {
                        c.Update();
                    }
                }
                catch(System.Exception e)
                {
                    UnityEngine.Debug.Log(e.StackTrace);
                    Logger.Log(e);
                }
            }
            Logger.Log("World thread successfully stopped");
            Logger.MainLog.Update(); // Rerun last log; FIX IN FUTURE, BAD PRACTICE
        });
        worldthread.Start();
    }

    internal void RemoveChunk(Chunk chunk)
    {
        _LoadedChunks.Remove(chunk);
    }

    public bool ChunkExists(int posx, int posz)
    {
        foreach (Chunk c in new List<Chunk>(_LoadedChunks))
        {
            if (c.PosX.Equals(posx) && c.PosZ.Equals(posz))
            {
                return true;
            }
        }
        return false;
    }

    public Chunk GetChunk(int posx, int posz)
    {
        foreach(Chunk c in new List<Chunk>(_LoadedChunks))
        {
            if(c.PosX.Equals(posx) && c.PosZ.Equals(posz))
            {
                return c;
            }
        }
        return new ErroredChunk(0, 0, this);
    }

    // Update is called once per frame
    public void Update()
    {
        foreach(Chunk c in _LoadedChunks)
        {
            c.OnUnityUpdate();
        }
    }

    public void OnApplicationQuit()
    {
        foreach(Chunk c in _LoadedChunks)
        {
            try
            {
                Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(c.PosX, c.PosZ), c.GetChunkSaveData());
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        IsRunning = false;
        Logger.Log("Stopping world thread");
    }
}
