using System.Collections.Generic;
using System.Threading;

using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World variables/objects
    public bool IsRunning;
    private bool ranOnce = false;
    // How far to render chunks from Playerpos
    private static readonly int renderDistanceFirstPass = 6;
    private static readonly int renderDistance = 4;
    private Thread worldThread;
    public Int3 PlayerStartingPos;
    public readonly List<Chunk> _LoadedChunks = new List<Chunk>();

    // World instance getter/setter
    public static World WorldInstance { get; private set; }

    // Instantiate World, Register loops, set Random Player start position
    public static void Instantiate()
    {
        WorldInstance = new World();
        MainLoopable.MLInstance.RegisterLoops(WorldInstance);
        System.Random r = new System.Random();
        WorldInstance.PlayerStartingPos = new Int3(r.Next(-1000, 1000), 62, r.Next(-1000, 1000));
    }

    // Start is called before the first frame update
    // Start world thread and generate Chunks in world thread
    public void Start()
    {
        this.IsRunning = true;
        this.worldThread = new Thread(() =>
        {
            Logger.Log("Initalizing world thread...");
            while(this.IsRunning)
            {
                try
                {
                    Int3 newChunkPos = this.PlayerStartingPos;
                    newChunkPos.ToChunkCoords();
                    Vector3 startingChunkPos = newChunkPos.GetVec3();
                    if(!this.ranOnce)
                    {
                        // For first time running world thread, for all Chunk Positions within Rendering Distance 
                        // check if chunk exists in file, if so get from file, if not Generate Chunk
                        for(int x = -(renderDistanceFirstPass - 1); x < renderDistanceFirstPass; x++)
                        {
                            for(int y = -(renderDistanceFirstPass - 1); y < renderDistanceFirstPass; y++)
                            {
                                for(int z = -(renderDistanceFirstPass - 1); z < renderDistanceFirstPass; z++)
                                {
                                    newChunkPos.SetPos(this.PlayerStartingPos);
                                    newChunkPos.AddPos(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
                                    newChunkPos.ToChunkCoords();
                                    if(Vector3.Distance(newChunkPos.GetVec3(), startingChunkPos) < (renderDistanceFirstPass - 1))
                                    {
                                        // If file exists for Chunk, read chunk data from file and add Chunk to _LoadedChunks
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)), this);
                                                this._LoadedChunks.Add(chunk);
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
                                            Chunk chunk = new Chunk(newChunkPos, this);
                                            this._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        Debug.Log("Finished Adding New Chunks First Round");
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector3.Distance(new Vector3(this._LoadedChunks[i].PosX * Chunk.ChunkSize, this._LoadedChunks[i].PosY * Chunk.ChunkSize, this._LoadedChunks[i].PosZ * Chunk.ChunkSize), new Vector3(this.PlayerStartingPos.x, this.PlayerStartingPos.y, this.PlayerStartingPos.z)) <= (renderDistance * Chunk.ChunkSize))
                            {
                                this._LoadedChunks[i].Update();
                            }
                        }
                        Debug.Log("Finished Updating Chunks First Round");
                        this.ranOnce = true;
                        Debug.Log("RanOnce set to TRUE");
                    }
                    // After ran once, continuously update
                    // If Player has been loaded in, keep generating chunks around player and degenerating chunks that are too far from player
                    if(GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        for(int x = -(renderDistance - 1); x < renderDistance; x++)
                        {
                            for(int y = -(renderDistance - 1); y < renderDistance; y++)
                            {
                                for(int z = -(renderDistance - 1); z < renderDistance; z++)
                                {
                                    newChunkPos.SetPos(currentPlayerPos);
                                    newChunkPos.AddPos(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
                                    newChunkPos.ToChunkCoords();
                                    if(!this.ChunkExists(newChunkPos))
                                    {
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)), this);
                                                this._LoadedChunks.Add(chunk);
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
                                            Chunk chunk = new Chunk(newChunkPos, this);
                                            this._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        // Iterate through Loaded Chunks and Degenerate if they are too far from player position
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector3.Distance(new Vector3(this._LoadedChunks[i].PosX * Chunk.ChunkSize, this._LoadedChunks[i].PosY * Chunk.ChunkSize, this._LoadedChunks[i].PosZ * Chunk.ChunkSize), new Vector3(currentPlayerPos.x, currentPlayerPos.y, currentPlayerPos.z)) <= (renderDistance * 1.5 * Chunk.ChunkSize))
                            {
                                this._LoadedChunks[i].Degenerate();
                            }
                        }
                        // Loop through loaded chunks and run Chunk.Update(): Draw/Update meshes to render
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector3.Distance(new Vector3(this._LoadedChunks[i].PosX * Chunk.ChunkSize, this._LoadedChunks[i].PosY * Chunk.ChunkSize, this._LoadedChunks[i].PosZ * Chunk.ChunkSize), new Vector3(currentPlayerPos.x, currentPlayerPos.y, currentPlayerPos.z)) <= ((renderDistance - 2) * Chunk.ChunkSize))
                            {
                                // Before update, if chunk has been set that it's neighbors need to update, tell those neighbors they need to update
                                // Neighbors will need to update meshes if a block is changed at the intersection of chunks to ensure no extra tris are rendered unseen
                                if(this._LoadedChunks[i].NeedToUpdateNegXNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX - 1, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX - 1, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegXNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdatePosXNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX + 1, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX + 1, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdatePosXNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdateNegYNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY - 1, this._LoadedChunks[i].PosZ))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY - 1, this._LoadedChunks[i].PosZ)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegYNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdatePosYNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY + 1, this._LoadedChunks[i].PosZ))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY + 1, this._LoadedChunks[i].PosZ)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdatePosYNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdateNegZNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ - 1))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ - 1)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdatePosZNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ + 1))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ + 1)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                }
                                this._LoadedChunks[i].Update();
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
        });
        this.worldThread.IsBackground = true;
        this.worldThread.Start();
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
                // Only bother saving chunks which have been modified by player
                if(this._LoadedChunks[i].HasBeenModified)
                {
                    Serializer.Serialize_ToFile_FullPath(FileManager.GetChunkString(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ), this._LoadedChunks[i].GetChunkSaveData());
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
        }
        this.IsRunning = false;
        Logger.Log("Stopping world thread...");
    }

    // Remove Chunk from world
    internal void RemoveChunk(Chunk chunk)
    {
        this._LoadedChunks.Remove(chunk);
    }

    // Check if Chunk currently exists
    public bool ChunkExists(int posx, int posy, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosY.Equals(posy) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return true;
            }
        }
        return false;
    }

    // Check if Chunk currently exists
    public bool ChunkExists(Int3 pos)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(pos.x) && this._LoadedChunks[i].PosY.Equals(pos.y) && this._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return true;
            }
        }
        return false;
    }

    // Get Chunk at location
    public Chunk GetChunk(int posx, int posy, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosY.Equals(posy) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return this._LoadedChunks[i];
            }
        }
        return new ErroredChunk(posx, posy, posz, this);
    }

    // Get Chunk at location
    public Chunk GetChunk(Int3 pos)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(pos.x) && this._LoadedChunks[i].PosY.Equals(pos.y) && this._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return this._LoadedChunks[i];
            }
        }
        return new ErroredChunk(pos.x, pos.y, pos.z, this);
    }

    // Get Chunk index position in Loaded Chunks
    public int GetChunkIndex(int posx, int posy, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosY.Equals(posy) && this._LoadedChunks[i].PosZ.Equals(posz))
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
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(pos.x) && this._LoadedChunks[i].PosY.Equals(pos.y) && this._LoadedChunks[i].PosZ.Equals(pos.z))
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
        Debug.Log($@"GetBlockFromWorldCoords: {x}, {y}, {z}");
        Logger.Log($@"GetBlockFromWorldCoords: {x}, {y}, {z}");
        Int3 chunkCoords = new Int3(x, y, z);
        chunkCoords.ToChunkCoords();
        Debug.Log($@"ToChunkCoords: {chunkCoords.ToString()}");
        Logger.Log($@"ToChunkCoords: {chunkCoords.ToString()}");
        Int3 pos = new Int3(x, y, z);
        pos.ToInternalChunkCoords(chunkCoords);
        Debug.Log($@"InternalChunkCoords: {pos.ToString()}");
        Logger.Log($@"InternalChunkCoords: {pos.ToString()}");
        if(this.ChunkExists(chunkCoords))
        {
            Chunk chunk = this.GetChunk(chunkCoords);
            Block b = chunk.GetBlockFromChunkInternalCoords(pos);
            return b;
        }
        Debug.Log($@"Chunk: {chunkCoords.ToString()} does not exist.");
        Logger.Log($@"Chunk: {chunkCoords.ToString()} does not exist.");
        throw new System.Exception("Trying to get Blocks from Chunk that doesn't exist.");
    }
}
