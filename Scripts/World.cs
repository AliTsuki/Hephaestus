using System.Collections.Generic;
using System.Threading;
using SharpNoise.Modules;
using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World variables/objects
    public bool IsRunning;
    private bool ranOnce = false;
    // How far to render chunks from Playerpos
    private readonly int renderDistanceFirstPass = 2;
    private readonly int renderDistance = 2;
    private Thread worldThread;
    public Int3 PlayerStartingPos;
    public readonly List<Chunk> _LoadedChunks = new List<Chunk>();

    public static readonly Perlin perlin = new Perlin()
    {
        Frequency = 0.015f,
        Lacunarity = 2f,
        OctaveCount = 4,
        Persistence = 0.25f,
        Seed = 0,
    };

    public static readonly RidgedMulti ridged = new RidgedMulti()
    {
        Frequency = GameManager.STATICRidgedFrequency,
        Lacunarity = GameManager.STATICRidgedLacunarity,
        OctaveCount = GameManager.STATICRidgedOctaveCount,
        Seed = GameManager.STATICRidgedSeed,
    };

    // World instance getter/setter
    public static World WorldInstance { get; private set; }

    // Instantiate World, Register loops, set Random Player start position
    public static void Instantiate()
    {
        WorldInstance = new World();
        MainLoopable.MLInstance.RegisterLoops(WorldInstance);
        System.Random r = new System.Random();
        //WorldInstance.PlayerStartingPos = new Int3(r.Next(-1000, 1000), 62, r.Next(-1000, 1000));
        WorldInstance.PlayerStartingPos = new Int3(0, 0, 0);
    }

    // Start is called before the first frame update
    // Start world thread and generate Chunks in world thread
    public void Start()
    {
        WorldInstance.IsRunning = true;
        WorldInstance.worldThread = 
        new Thread(() =>
        {
            Logger.Log("Initalizing world thread...");
            while(WorldInstance.IsRunning)
            {
                try
                {
                    Int3 startingChunkPos = WorldInstance.PlayerStartingPos;
                    startingChunkPos.ToChunkCoords();
                    if(!WorldInstance.ranOnce)
                    {
                        // For first time running world thread, for all Chunk Positions within Rendering Distance 
                        // check if chunk exists in file, if so get from file, if not Generate Chunk
                        for(int x = -WorldInstance.renderDistanceFirstPass; x < WorldInstance.renderDistanceFirstPass; x++)
                        {
                            for(int y = -WorldInstance.renderDistanceFirstPass; y < WorldInstance.renderDistanceFirstPass; y++)
                            {
                                for(int z = -WorldInstance.renderDistanceFirstPass; z < WorldInstance.renderDistanceFirstPass; z++)
                                {
                                    Int3 newChunkPos = WorldInstance.PlayerStartingPos;
                                    newChunkPos.AddPos(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
                                    newChunkPos.ToChunkCoords();
                                    //if(Vector3.Distance(newChunkPos.GetVec3(), startingChunkPos.GetVec3()) <= renderDistanceFirstPass)
                                    //{
                                        // If file exists for Chunk, read chunk data from file and add Chunk to _LoadedChunks
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                            WorldInstance._LoadedChunks.Add(chunk);
                                                chunk.Start();
                                            }
                                            catch(System.Exception e)
                                            {
                                                Debug.Log(e.ToString());
                                                Logger.Log(e);
                                            }
                                        }
                                        else
                                        {
                                            Chunk chunk = new Chunk(newChunkPos);
                                        WorldInstance._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                    //}
                                }
                            }
                        }
                        Debug.Log("Finished Adding New Chunks First Round");
                        Debug.Log($@"Loaded Chunks: {WorldInstance._LoadedChunks.Count}");
                        Logger.Log("Finished Adding New Chunks First Round");
                        Logger.Log($@"Loaded Chunks: {WorldInstance._LoadedChunks.Count}");
                        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
                        {
                            //Int3 chunkPos = new Int3(_LoadedChunks[i].PosX, _LoadedChunks[i].PosY, _LoadedChunks[i].PosZ);
                            //if(Vector3.Distance(chunkPos.GetVec3(), startingChunkPos.GetVec3()) <= renderDistance)
                            //{
                                if(WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].NegXNeighbor) && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].PosXNeighbor)
                                 && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].NegYNeighbor) && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].PosYNeighbor)
                                 && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].NegZNeighbor) && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].PosZNeighbor))
                                {
                                    Debug.Log("////////////////////////////////////////////////////////////////////////////////");
                                    Logger.Log("////////////////////////////////////////////////////////////////////////////////");
                                    Debug.Log($@"Chunk HAS ALL neighbors: C_{WorldInstance._LoadedChunks[i].PosX}_{WorldInstance._LoadedChunks[i].PosY}_{WorldInstance._LoadedChunks[i].PosZ}");
                                    Logger.Log($@"Chunk HAS ALL neighbors: C_{WorldInstance._LoadedChunks[i].PosX}_{WorldInstance._LoadedChunks[i].PosY}_{WorldInstance._LoadedChunks[i].PosZ}");
                                    Debug.Log($@"Neighbors Checked: ({WorldInstance._LoadedChunks[i].NegXNeighbor}), ({WorldInstance._LoadedChunks[i].PosXNeighbor}), ({WorldInstance._LoadedChunks[i].NegYNeighbor}), ({WorldInstance._LoadedChunks[i].PosYNeighbor}), ({WorldInstance._LoadedChunks[i].NegZNeighbor}), ({WorldInstance._LoadedChunks[i].PosZNeighbor})");
                                    Logger.Log($@"Neighbors Checked: ({WorldInstance._LoadedChunks[i].NegXNeighbor}), ({WorldInstance._LoadedChunks[i].PosXNeighbor}), ({WorldInstance._LoadedChunks[i].NegYNeighbor}), ({WorldInstance._LoadedChunks[i].PosYNeighbor}), ({WorldInstance._LoadedChunks[i].NegZNeighbor}), ({WorldInstance._LoadedChunks[i].PosZNeighbor})");
                                WorldInstance._LoadedChunks[i].Update();
                                    Debug.Log("////////////////////////////////////////////////////////////////////////////////");
                                    Logger.Log("////////////////////////////////////////////////////////////////////////////////");
                            }
                                else
                                {
                                    Debug.Log($@"Chunk does not have all neighbors: C_{WorldInstance._LoadedChunks[i].PosX}_{WorldInstance._LoadedChunks[i].PosY}_{WorldInstance._LoadedChunks[i].PosZ}");
                                    Logger.Log($@"Chunk does not have all neighbors: C_{WorldInstance._LoadedChunks[i].PosX}_{WorldInstance._LoadedChunks[i].PosY}_{WorldInstance._LoadedChunks[i].PosZ}");
                                    Debug.Log($@"Neighbors Checked: ({WorldInstance._LoadedChunks[i].NegXNeighbor}), ({WorldInstance._LoadedChunks[i].PosXNeighbor}), ({WorldInstance._LoadedChunks[i].NegYNeighbor}), ({WorldInstance._LoadedChunks[i].PosYNeighbor}), ({WorldInstance._LoadedChunks[i].NegZNeighbor}), ({WorldInstance._LoadedChunks[i].PosZNeighbor})");
                                    Logger.Log($@"Neighbors Checked: ({WorldInstance._LoadedChunks[i].NegXNeighbor}), ({WorldInstance._LoadedChunks[i].PosXNeighbor}), ({WorldInstance._LoadedChunks[i].NegYNeighbor}), ({WorldInstance._LoadedChunks[i].PosYNeighbor}), ({WorldInstance._LoadedChunks[i].NegZNeighbor}), ({WorldInstance._LoadedChunks[i].PosZNeighbor})");
                                }
                            //}
                        }
                        Debug.Log("Finished Updating Chunks First Round");
                        Logger.Log("Finished Updating Chunks First Round");
                        WorldInstance.ranOnce = true;
                        Debug.Log("Finished Starter Zone Initialization");
                        Logger.Log("Finished Starter Zone Initialization");
                    }
                    // After ran once, continuously update
                    // If Player has been loaded in, keep generating chunks around player and degenerating chunks that are too far from player
                    if(GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        for(int x = -WorldInstance.renderDistance; x < WorldInstance.renderDistance; x++)
                        {
                            for(int y = -WorldInstance.renderDistance; y < WorldInstance.renderDistance; y++)
                            {
                                for(int z = -WorldInstance.renderDistance; z < WorldInstance.renderDistance; z++)
                                {
                                    Int3 newChunkPos = currentPlayerPos;
                                    newChunkPos.AddPos(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
                                    newChunkPos.ToChunkCoords();
                                    if(!WorldInstance.ChunkExists(newChunkPos))
                                    {
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                                WorldInstance._LoadedChunks.Add(chunk);
                                                chunk.Start();
                                            }
                                            catch(System.Exception e)
                                            {
                                                Debug.Log(e.ToString());
                                                Logger.Log(e);
                                            }
                                        }
                                        else
                                        {
                                            Chunk chunk = new Chunk(newChunkPos);
                                            WorldInstance._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        // Iterate through Loaded Chunks and Degenerate if they are too far from player position
                        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
                        {
                            Int3 chunkPos = new Int3(WorldInstance._LoadedChunks[i].PosX * Chunk.ChunkSize, WorldInstance._LoadedChunks[i].PosY * Chunk.ChunkSize, WorldInstance._LoadedChunks[i].PosZ * Chunk.ChunkSize);
                            if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) <= (WorldInstance.renderDistance * 1.5 * Chunk.ChunkSize))
                            {
                                WorldInstance._LoadedChunks[i].Degenerate();
                            }
                        }
                        // Loop through loaded chunks and run Chunk.Update(): Draw/Update meshes to render
                        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
                        {
                            Int3 chunkPos = new Int3(WorldInstance._LoadedChunks[i].PosX * Chunk.ChunkSize, WorldInstance._LoadedChunks[i].PosY * Chunk.ChunkSize, WorldInstance._LoadedChunks[i].PosZ * Chunk.ChunkSize);
                            if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) <= (WorldInstance.renderDistance * Chunk.ChunkSize))
                            {
                                // Before update, if chunk has been set that it's neighbors need to update, tell those neighbors they need to update
                                // Neighbors will need to update meshes if a block is changed at the intersection of chunks to ensure no extra tris are rendered unseen
                                if(WorldInstance._LoadedChunks[i].NeedToUpdateNegXNeighbor && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].NegXNeighbor))
                                {
                                    WorldInstance._LoadedChunks[WorldInstance.GetChunkIndex(WorldInstance._LoadedChunks[i].NegXNeighbor)].NeedToUpdate = true;
                                    WorldInstance._LoadedChunks[i].NeedToUpdateNegXNeighbor = false;
                                }
                                if(WorldInstance._LoadedChunks[i].NeedToUpdatePosXNeighbor && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].PosXNeighbor))
                                {
                                    WorldInstance._LoadedChunks[WorldInstance.GetChunkIndex(WorldInstance._LoadedChunks[i].PosXNeighbor)].NeedToUpdate = true;
                                    WorldInstance._LoadedChunks[i].NeedToUpdatePosXNeighbor = false;
                                }
                                if(WorldInstance._LoadedChunks[i].NeedToUpdateNegYNeighbor && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].NegYNeighbor))
                                {
                                    WorldInstance._LoadedChunks[WorldInstance.GetChunkIndex(WorldInstance._LoadedChunks[i].NegYNeighbor)].NeedToUpdate = true;
                                    WorldInstance._LoadedChunks[i].NeedToUpdateNegYNeighbor = false;
                                }
                                if(WorldInstance._LoadedChunks[i].NeedToUpdatePosYNeighbor && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].PosYNeighbor))
                                {
                                    WorldInstance._LoadedChunks[WorldInstance.GetChunkIndex(WorldInstance._LoadedChunks[i].PosYNeighbor)].NeedToUpdate = true;
                                    WorldInstance._LoadedChunks[i].NeedToUpdatePosYNeighbor = false;
                                }
                                if(WorldInstance._LoadedChunks[i].NeedToUpdateNegZNeighbor && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].NegZNeighbor))
                                {
                                    WorldInstance._LoadedChunks[WorldInstance.GetChunkIndex(WorldInstance._LoadedChunks[i].NegZNeighbor)].NeedToUpdate = true;
                                    WorldInstance._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                }
                                if(WorldInstance._LoadedChunks[i].NeedToUpdatePosZNeighbor && WorldInstance.ChunkExists(WorldInstance._LoadedChunks[i].PosZNeighbor))
                                {
                                    WorldInstance._LoadedChunks[WorldInstance.GetChunkIndex(WorldInstance._LoadedChunks[i].PosZNeighbor)].NeedToUpdate = true;
                                    WorldInstance._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                }
                                WorldInstance._LoadedChunks[i].Update();
                            }
                        }
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
        })
        {
            IsBackground = true
        };
        WorldInstance.worldThread.Start();
    }

    // Update is called once per frame
    // Update Chunks
    public void Update()
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            WorldInstance._LoadedChunks[i].OnUnityUpdate();
        }
    }

    // On Application Quit, save Chunks to file and stop world thread
    public void OnApplicationQuit()
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            try
            {
                Int3 chunkPos = new Int3(WorldInstance._LoadedChunks[i].PosX, WorldInstance._LoadedChunks[i].PosY, WorldInstance._LoadedChunks[i].PosZ);
                // Only bother saving chunks which have been modified by player
                if(WorldInstance._LoadedChunks[i].HasBeenModified)
                {
                    Serializer.Serialize_ToFile_FullPath(FileManager.GetChunkString(chunkPos), WorldInstance._LoadedChunks[i].GetChunkSaveData());
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
        }
        WorldInstance.IsRunning = false;
        Logger.Log("Stopping world thread...");
    }

    // Remove Chunk from world
    public void RemoveChunk(Chunk chunk)
    {
        WorldInstance._LoadedChunks.Remove(chunk);
    }

    // Check if Chunk currently exists
    public bool ChunkExists(int posx, int posy, int posz)
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            if(WorldInstance._LoadedChunks[i].PosX.Equals(posx) && WorldInstance._LoadedChunks[i].PosY.Equals(posy) && WorldInstance._LoadedChunks[i].PosZ.Equals(posz))
            {
                return true;
            }
        }
        return false;
    }

    // Check if Chunk currently exists
    public bool ChunkExists(Int3 pos)
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            if(WorldInstance._LoadedChunks[i].PosX.Equals(pos.x) && WorldInstance._LoadedChunks[i].PosY.Equals(pos.y) && WorldInstance._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return true;
            }
        }
        return false;
    }

    // Get Chunk at location
    public Chunk GetChunk(int posx, int posy, int posz)
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            if(WorldInstance._LoadedChunks[i].PosX.Equals(posx) && WorldInstance._LoadedChunks[i].PosY.Equals(posy) && WorldInstance._LoadedChunks[i].PosZ.Equals(posz))
            {
                return WorldInstance._LoadedChunks[i];
            }
        }
        return new ErroredChunk(posx, posy, posz);
    }

    // Get Chunk at location
    public Chunk GetChunk(Int3 pos)
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            if(WorldInstance._LoadedChunks[i].PosX.Equals(pos.x) && WorldInstance._LoadedChunks[i].PosY.Equals(pos.y) && WorldInstance._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return WorldInstance._LoadedChunks[i];
            }
        }
        return new ErroredChunk(pos.x, pos.y, pos.z);
    }

    // Get Chunk index position in Loaded Chunks
    public int GetChunkIndex(int posx, int posy, int posz)
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            if(WorldInstance._LoadedChunks[i].PosX.Equals(posx) && WorldInstance._LoadedChunks[i].PosY.Equals(posy) && WorldInstance._LoadedChunks[i].PosZ.Equals(posz))
            {
                return i;
            }
        }
        Logger.Log("Trying to get Chunk Index of Chunk that doesn't exist.");
        throw new System.Exception("Trying to get Chunk Index of Chunk that doesn't exist.");
    }

    // Get Chunk index position in Loaded Chunks
    public int GetChunkIndex(Int3 pos)
    {
        for(int i = 0; i < WorldInstance._LoadedChunks.Count; i++)
        {
            if(WorldInstance._LoadedChunks[i].PosX.Equals(pos.x) && WorldInstance._LoadedChunks[i].PosY.Equals(pos.y) && WorldInstance._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return i;
            }
        }
        Logger.Log("Trying to get Chunk Index of Chunk that doesn't exist.");
        throw new System.Exception("Trying to get Chunk Index of Chunk that doesn't exist.");
    }

    // Get Block Type at given World Coords
    public Block GetBlockFromWorldCoords(int x, int y, int z)
    {
        Int3 chunkCoords = new Int3(x, y, z);
        chunkCoords.ToChunkCoords();
        Debug.Log($@"Checking Chunk: C_{chunkCoords.x}_{chunkCoords.y}_{chunkCoords.z}");
        Logger.Log($@"Checking Chunk: C_{chunkCoords.x}_{chunkCoords.y}_{chunkCoords.z}");
        Int3 pos = new Int3(x, y, z);
        pos.ToInternalChunkCoords();
        Debug.Log($@"Checking Block at internal pos: {pos.x}, {pos.y}, {pos.z}");
        Logger.Log($@"Checking Block at internal pos: {pos.x}, {pos.y}, {pos.z}");
        if(WorldInstance.ChunkExists(chunkCoords))
        {
            Block b = WorldInstance.GetChunk(chunkCoords).GetBlockFromChunkInternalCoords(pos);
            return b;
        }
        else
        {
            Debug.Log($@"Chunk: {chunkCoords.ToString()} does not exist.");
            Logger.Log($@"Chunk: {chunkCoords.ToString()} does not exist.");
            throw new System.Exception("Trying to get Blocks from Chunk that doesn't exist.");
        }
    }
}
