using System.Collections.Generic;
using System.Threading;
using SharpNoise.Modules;
using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World variables/objects
    public bool IsRunning;
    private bool initialChunkStartComplete = false;
    private bool initialChunkUpdateComplete = false;
    private Thread worldGenThread;
    private Thread worldUpdateThread;
    public Int3 WorldStartPos;
    public volatile List<Chunk> _LoadedChunks = new List<Chunk>();
    // Chunks to render around player, FirstPass is initialization, renderDistance is during regular gameplay
    public readonly int renderDistanceFirstPass = 7;
    private readonly int renderDistance = 5;
    // Noise generators
    public static readonly Perlin perlin = new Perlin()
    {
        Frequency = GameManager.PerlinFrequency,
        Lacunarity = GameManager.PerlinLacunarity,
        OctaveCount = GameManager.PerlinOctaveCount,
        Persistence = GameManager.PerlinPersistence,
        Seed = GameManager.PerlinSeed,
    };
    public static readonly RidgedMulti ridged = new RidgedMulti()
    {
        Frequency = GameManager.RidgedFrequency,
        Lacunarity = GameManager.RidgedLacunarity,
        OctaveCount = GameManager.RidgedOctaveCount,
        Seed = GameManager.RidgedSeed,
    };
    // World instance getter/setter
    public static World Instance { get; private set; }

    // Instantiate World, Register loops, set Random World start position
    public static void Instantiate()
    {
        Instance = new World();
        MainLoopable.Instance.RegisterLoops(Instance);
        System.Random r = new System.Random();
        Instance.WorldStartPos = new Int3(r.Next(-1000, 1000), 0, r.Next(-1000, 1000));
    }

    // Start is called before the first frame update
    // Start world threads
    public void Start()
    {
        this.IsRunning = true;
        // Thread for generating and degenerating chunks
        this.worldGenThread = 
        new Thread(() =>
        {
            Logger.Log($@"{GameManager.time}: Initalizing world gen thread...");
            while(this.IsRunning)
            {
                try
                {
                    // For first time running world gen thread, for all Chunks within Rendering Distance First Pass
                    // Check if chunk exists in save file, if so get from file, if not Generate Chunk
                    Int3 startingChunkPos = this.WorldStartPos;
                    startingChunkPos.ToChunkCoords();
                    if(!this.initialChunkStartComplete && !GameManager.PlayerLoaded())
                    {
                        for(int x = -this.renderDistanceFirstPass; x < this.renderDistanceFirstPass; x++)
                        {
                            for(int y = -this.renderDistanceFirstPass; y < this.renderDistanceFirstPass; y++)
                            {
                                for(int z = -this.renderDistanceFirstPass; z < this.renderDistanceFirstPass; z++)
                                {
                                    Int3 newChunkPos = this.WorldStartPos;
                                    newChunkPos.AddPos(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
                                    newChunkPos.ToChunkCoords();
                                    // x,y,z for loop makes a cube of chunks of renderDistanceFirstPass^3, distance function below cuts that into a smaller sphere, saves 30+% in generation time
                                    if(Vector3.Distance(newChunkPos.GetVec3(), startingChunkPos.GetVec3()) <= this.renderDistanceFirstPass)
                                    {
                                        // If save file exists for Chunk, read chunk data from file and add Chunk to World
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                            this._LoadedChunks.Add(chunk);
                                                chunk.Start();
                                            }
                                            catch(System.Exception e)
                                            {
                                                Debug.Log(e.ToString());
                                                Logger.Log(e);
                                            }
                                        }
                                        // If no save file for chunk, generate new Chunk and add to World
                                        else
                                        {
                                            Chunk chunk = new Chunk(newChunkPos);
                                            this._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        this.initialChunkStartComplete = true;
                        Debug.Log($@"{GameManager.time}: Finished Starter Zone Generation Initialization");
                        Logger.Log($@"{GameManager.time}: Finished Starter Zone Generation Initialization");
                    }
                    // After initial chunk generatio and player loaded into world, continuously update around player
                    // If Player has been loaded in, keep generating chunks around player and degenerating chunks that are too far from player
                    if(this.initialChunkStartComplete && GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        for(int x = -this.renderDistance; x < this.renderDistance; x++)
                        {
                            for(int y = -this.renderDistance; y < this.renderDistance; y++)
                            {
                                for(int z = -this.renderDistance; z < this.renderDistance; z++)
                                {
                                    Int3 newChunkPos = currentPlayerPos;
                                    newChunkPos.AddPos(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
                                    newChunkPos.ToChunkCoords();
                                    // If currently pointed to Chunk hasn't already been added to World
                                    if(!this.ChunkExists(newChunkPos))
                                    {
                                        // If save file exists for Chunk, read chunk data from file and add Chunk to World
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                                this._LoadedChunks.Add(chunk);
                                                chunk.Start();
                                            }
                                            catch(System.Exception e)
                                            {
                                                Debug.Log(e.ToString());
                                                Logger.Log(e);
                                            }
                                        }
                                        // If no save file for chunk, generate new Chunk and add to World
                                        else
                                        {
                                            Chunk chunk = new Chunk(newChunkPos);
                                            this._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        // Iterate through Loaded Chunks and Degenerate if they are too far from current player position
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            Int3 chunkPos = new Int3(this._LoadedChunks[i].PosX * Chunk.ChunkSize, this._LoadedChunks[i].PosY * Chunk.ChunkSize, this._LoadedChunks[i].PosZ * Chunk.ChunkSize);
                            if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) >= (this.renderDistance * 1.5 * Chunk.ChunkSize))
                            {
                                this._LoadedChunks[i].Degenerate();
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
            Logger.Log($@"{GameManager.time}: World gen thread successfully stopped.");
        });
        // Thread for updating chunks
        this.worldUpdateThread =
        new Thread(() =>
        {
            Logger.Log($@"{GameManager.time}: Initalizing world update thread...");
            while(this.IsRunning)
            {
                try
                {
                    // After initial Chunk generation, and before player is instantiated, update all Chunks that have existing neighbors
                    // Neighbor existence is required to avoid problems in meshing the edges of the Chunk when neighbors don't exist yet
                    if(this.initialChunkStartComplete && !this.initialChunkUpdateComplete && !GameManager.PlayerLoaded())
                    {
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(this.ChunkExists(this._LoadedChunks[i].NegXNeighbor) && this.ChunkExists(this._LoadedChunks[i].PosXNeighbor)
                                && this.ChunkExists(this._LoadedChunks[i].NegYNeighbor) && this.ChunkExists(this._LoadedChunks[i].PosYNeighbor)
                                && this.ChunkExists(this._LoadedChunks[i].NegZNeighbor) && this.ChunkExists(this._LoadedChunks[i].PosZNeighbor))
                            {
                                this._LoadedChunks[i].Update();
                            }
                        }
                        this.initialChunkUpdateComplete = true;
                        Debug.Log($@"{GameManager.time}: Finished Starter Zone Meshing Initialization");
                        Logger.Log($@"{GameManager.time}: Finished Starter Zone Meshing Initialization");
                    }
                    // After initial Chunks have been generated and meshed, try instantiating the player character
                    if(this.initialChunkStartComplete && this.initialChunkUpdateComplete && !GameManager.PlayerLoaded())
                    {
                        Int3 PlayerStartPos = this.WorldStartPos;
                        PlayerStartPos = MathHelper.GetPlayerStartPosition(PlayerStartPos);
                        Int3 PlayerStartChunkPos = PlayerStartPos;
                        PlayerStartChunkPos.ToChunkCoords();
                        Chunk chunk = this.GetChunk(PlayerStartChunkPos);
                        GameObject go = chunk.go;
                        GameManager.Instance.StartPlayer(PlayerStartPos.GetVec3(), go);
                    }
                    // After initial Chunk gen and meshing, and player start, continuously update loaded Chunks around current player position
                    if(this.initialChunkStartComplete &&  this.initialChunkUpdateComplete && GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            Int3 chunkPos = new Int3(this._LoadedChunks[i].PosX * Chunk.ChunkSize, this._LoadedChunks[i].PosY * Chunk.ChunkSize, this._LoadedChunks[i].PosZ * Chunk.ChunkSize);
                            if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) <= (this.renderDistance * Chunk.ChunkSize))
                            {
                                if(this.ChunkExists(this._LoadedChunks[i].NegXNeighbor) && this.ChunkExists(this._LoadedChunks[i].PosXNeighbor)
                                && this.ChunkExists(this._LoadedChunks[i].NegYNeighbor) && this.ChunkExists(this._LoadedChunks[i].PosYNeighbor)
                                && this.ChunkExists(this._LoadedChunks[i].NegZNeighbor) && this.ChunkExists(this._LoadedChunks[i].PosZNeighbor))
                                {
                                    // Before update, if Chunk has been set that its neighbors need to update, tell those neighbors they need to update
                                    // Neighbors will need to update meshes if a block is changed at the intersection of chunks to avoid holes between chunks
                                    if(this._LoadedChunks[i].NeedToUpdateNegXNeighbor)
                                    {
                                        this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].NegXNeighbor)].NeedToUpdate = true;
                                        this._LoadedChunks[i].NeedToUpdateNegXNeighbor = false;
                                    }
                                    if(this._LoadedChunks[i].NeedToUpdatePosXNeighbor)
                                    {
                                        this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosXNeighbor)].NeedToUpdate = true;
                                        this._LoadedChunks[i].NeedToUpdatePosXNeighbor = false;
                                    }
                                    if(this._LoadedChunks[i].NeedToUpdateNegYNeighbor)
                                    {
                                        this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].NegYNeighbor)].NeedToUpdate = true;
                                        this._LoadedChunks[i].NeedToUpdateNegYNeighbor = false;
                                    }
                                    if(this._LoadedChunks[i].NeedToUpdatePosYNeighbor)
                                    {
                                        this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosYNeighbor)].NeedToUpdate = true;
                                        this._LoadedChunks[i].NeedToUpdatePosYNeighbor = false;
                                    }
                                    if(this._LoadedChunks[i].NeedToUpdateNegZNeighbor)
                                    {
                                        this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].NegZNeighbor)].NeedToUpdate = true;
                                        this._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                    }
                                    if(this._LoadedChunks[i].NeedToUpdatePosZNeighbor)
                                    {
                                        this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosZNeighbor)].NeedToUpdate = true;
                                        this._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                    }
                                    this._LoadedChunks[i].Update();
                                }
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
            Logger.Log($@"{GameManager.time}: World update thread successfully stopped.");
        });
        // Start threads defined above
        this.worldGenThread.Start();
        this.worldUpdateThread.Start();
    }
    

    // Update is called once per frame
    // Update Chunks
    public void Update()
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            this._LoadedChunks[i].OnUnityUpdate();
        }
        Logger.MainLog.Update();
    }

    // On Application Quit, save Chunks to file and stop world threads
    public void OnApplicationQuit()
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            try
            {
                Int3 chunkPos = new Int3(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosY, this._LoadedChunks[i].PosZ);
                // Only save chunks which have been modified by player
                if(this._LoadedChunks[i].HasBeenModified)
                {
                    Serializer.Serialize_ToFile_FullPath(FileManager.GetChunkString(chunkPos), this._LoadedChunks[i].GetChunkSaveData());
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
        }
        this.IsRunning = false;
        Logger.Log($@"{GameManager.time}: Stopping world threads...");
    }

    // Remove Chunk from world, given Chunk
    public void RemoveChunk(Chunk chunk)
    {
        this._LoadedChunks.Remove(chunk);
    }

    // Check if Chunk currently exists, given Chunk.PosX, PosY, PosZ
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

    // Check if Chunk currently exists, given Chunk.PosX, PosY, PosZ
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

    // Get Chunk at location, given Chunk.PosX, PosY, PosZ
    public Chunk GetChunk(int posx, int posy, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosY.Equals(posy) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return this._LoadedChunks[i];
            }
        }
        return new ErroredChunk(posx, posy, posz);
    }

    // Get Chunk at location, given Chunk.PosX, PosY, PosZ
    public Chunk GetChunk(Int3 pos)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(pos.x) && this._LoadedChunks[i].PosY.Equals(pos.y) && this._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return this._LoadedChunks[i];
            }
        }
        return new ErroredChunk(pos.x, pos.y, pos.z);
    }

    // Get Chunk index position in Loaded Chunks, given Chunk.PosX, PosY, PosZ
    public int GetChunkIndex(int posx, int posy, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosY.Equals(posy) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return i;
            }
        }
        Logger.Log($@"{GameManager.time}: Trying to get Chunk Index of Chunk that doesn't exist.");
        throw new System.Exception("Trying to get Chunk Index of Chunk that doesn't exist.");
    }

    // Get Chunk index position in Loaded Chunks, given Chunk.PosX, PosY, PosZ
    public int GetChunkIndex(Int3 pos)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(pos.x) && this._LoadedChunks[i].PosY.Equals(pos.y) && this._LoadedChunks[i].PosZ.Equals(pos.z))
            {
                return i;
            }
        }
        Logger.Log($@"{GameManager.time}: Trying to get Chunk Index of Chunk that doesn't exist.");
        throw new System.Exception("Trying to get Chunk Index of Chunk that doesn't exist.");
    }

    // Get Block, given World Coords x, y, z
    public Block GetBlockFromWorldCoords(int x, int y, int z)
    {
        Int3 chunkCoords = new Int3(x, y, z);
        chunkCoords.ToChunkCoords();
        Int3 pos = new Int3(x, y, z);
        pos.ToInternalChunkCoords();
        if(this.ChunkExists(chunkCoords))
        {
            Block b = this.GetChunk(chunkCoords).GetBlockFromChunkInternalCoords(pos);
            return b;
        }
        else
        {
            Debug.Log($@"{GameManager.time}: Chunk: {chunkCoords.ToString()} does not exist.");
            Logger.Log($@"{GameManager.time}: Chunk: {chunkCoords.ToString()} does not exist.");
            throw new System.Exception("Trying to get Blocks from Chunk that doesn't exist.");
        }
    }
}
