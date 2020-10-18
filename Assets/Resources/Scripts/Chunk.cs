using System;

using UnityEngine;

namespace OLD
{
    // Class containing Chunk functions
    public class Chunk : ITickable
    {
        // Chunk fields
        protected bool hasGenerated = false;
        protected bool hasDrawn = false;
        protected bool hasRendered = false;
        protected bool drawLock = false;
        protected bool renderingLock = false;
        public bool NeedToUpdate = false;
        public bool HasBeenModified = false;
        public Int3 NegXNeighbor;
        public Int3 PosXNeighbor;
        public Int3 NegYNeighbor;
        public Int3 PosYNeighbor;
        public Int3 NegZNeighbor;
        public Int3 PosZNeighbor;
        public bool NeedToUpdateNegXNeighbor = false;
        public bool NeedToUpdatePosXNeighbor = false;
        public bool NeedToUpdateNegYNeighbor = false;
        public bool NeedToUpdatePosYNeighbor = false;
        public bool NeedToUpdateNegZNeighbor = false;
        public bool NeedToUpdatePosZNeighbor = false;
        private MeshData data;
        public GameObject GO;
        private Block[,,] blocks;
        private Biome biome;
        // Chunk size in blocks
        public const int ChunkSize = 16;
        // Chunk position getter/setter
        public Int3 Pos { get; private set; }

        // Chunk constructor, given Int3
        public Chunk(Int3 _pos)
        {
            this.Pos = _pos;
            this.NegXNeighbor.SetPos(this.Pos.x - 1, this.Pos.y, this.Pos.z);
            this.PosXNeighbor.SetPos(this.Pos.x + 1, this.Pos.y, this.Pos.z);
            this.NegYNeighbor.SetPos(this.Pos.x, this.Pos.y - 1, this.Pos.z);
            this.PosYNeighbor.SetPos(this.Pos.x, this.Pos.y + 1, this.Pos.z);
            this.NegZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z - 1);
            this.PosZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z + 1);
        }

        // Chunk constructor for saved data, given Int3 and int 3D Array data
        public Chunk(Int3 _pos, int[,,] _data)
        {
            this.Pos = _pos;
            this.NegXNeighbor.SetPos(this.Pos.x - 1, this.Pos.y, this.Pos.z);
            this.PosXNeighbor.SetPos(this.Pos.x + 1, this.Pos.y, this.Pos.z);
            this.NegYNeighbor.SetPos(this.Pos.x, this.Pos.y - 1, this.Pos.z);
            this.PosYNeighbor.SetPos(this.Pos.x, this.Pos.y + 1, this.Pos.z);
            this.NegZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z - 1);
            this.PosZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z + 1);
            this.hasGenerated = true;
            this.LoadChunkFromData(_data);
        }

        // Chunk Start: Generate Blocks in Chunk from noise
        public virtual void Start()
        {
            if(!this.hasGenerated)
            {
                this.GenerateBlocks();
                //Debug.Log($@"{GameManager.time}: Generated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
                //Logger.Log($@"{GameManager.time}: Generated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
            }
        }

        // Chunk Tick
        public void Tick()
        {
            
        }

        // Chunk Update: Create Mesh for Chunk
        public virtual void Update()
        {
            if(this.NeedToUpdate)
            {
                if(!this.drawLock && !this.renderingLock)
                {
                    this.hasDrawn = false;
                    this.hasRendered = false;
                    this.NeedToUpdate = false;
                }
            }
            if(this.hasGenerated && !this.hasDrawn && !this.drawLock)
            {
                this.GenerateMesh();
                //Debug.Log($@"{GameManager.time}: Meshed chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
                //Logger.Log($@"{GameManager.time}: Meshed chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
            }
        }

        // Chunk On Unity Update: Render Chunks / Create Chunk GameObjects and Assign Meshes
        public virtual void OnUnityUpdate()
        {
            if(this.hasGenerated && !this.hasRendered && this.hasDrawn && !this.renderingLock)
            {
                this.ApplyMesh();
                //Debug.Log($@"{GameManager.time}: Rendered chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
                //Logger.Log($@"{GameManager.time}: Rendered chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
            }
        }

        private void GenerateBlocks()
        {
            this.biome = Biome.GetBiome(this.Pos);
            this.blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
            System.Random random = new System.Random();
            int cutoffMargin;
            for(int x = 0; x < ChunkSize; x++)
            {
                for(int y = 0; y < ChunkSize; y++)
                {
                    for(int z = 0; z < ChunkSize; z++)
                    {
                        Int3 pos = new Int3(x, y, z);
                        pos.ChunkInternalCoordsToWorldCoords(this.Pos);
                        float perlin = this.GetNoise(pos);
                        float perlinCave = this.GetNoiseCave(pos);
                        // Above Ground Generation
                        // Air Layer
                        if(perlin > this.biome.AirAndLandIntersectionCutoff)
                        {
                            this.blocks[x, y, z] = Block.Air;
                        }
                        // Top layer
                        else if(perlin < this.biome.AirAndLandIntersectionCutoff && perlin > this.biome.LandTopLayerCutoff)
                        {
                            cutoffMargin = random.Next(-4, 4);
                            if(cutoffMargin + pos.y > 110)
                            {
                                this.blocks[x, y, z] = Block.Snow;
                            }
                            else if(cutoffMargin + pos.y < 110 && cutoffMargin + pos.y > 100)
                            {
                                this.blocks[x, y, z] = Block.Stone;
                            }
                            else if(cutoffMargin + pos.y < 100 && cutoffMargin + pos.y > 90)
                            {
                                this.blocks[x, y, z] = Block.Dirt;
                            }
                            else if(cutoffMargin + pos.y < 90 && cutoffMargin + pos.y > 62)
                            {
                                this.blocks[x, y, z] = Block.Grass;
                            }
                            else if(cutoffMargin + pos.y < 62 && cutoffMargin + pos.y > 30)
                            {
                                this.blocks[x, y, z] = Block.Sand;
                            }
                            else
                            {
                                this.blocks[x, y, z] = Block.Stone;
                            }
                        }
                        // Secondary Layer
                        else if(perlin < this.biome.LandTopLayerCutoff && perlin > this.biome.Land2NDLayerCutoff)
                        {
                            cutoffMargin = random.Next(-4, 4);
                            if(cutoffMargin + pos.y > 100)
                            {
                                this.blocks[x, y, z] = Block.Stone;
                            }
                            else if(cutoffMargin + pos.y < 100 && cutoffMargin + pos.y > 62)
                            {
                                this.blocks[x, y, z] = Block.Dirt;
                            }
                            else if(cutoffMargin + pos.y < 62 && cutoffMargin + pos.y > 30)
                            {
                                this.blocks[x, y, z] = Block.Dirt;
                            }
                            else
                            {
                                this.blocks[x, y, z] = Block.Stone;
                            }
                        }
                        // Inner Layer
                        else
                        {
                            this.blocks[x, y, z] = Block.Stone;
                        }
                        // Cave Generation
                        if(perlinCave > this.biome.CaveCutoff)
                        {
                            this.blocks[x, y, z] = Block.Air;
                        }
                    }
                }
            }
            this.hasGenerated = true;
        }

        private void GenerateMesh()
        {
            this.drawLock = true;
            this.data = new MeshData();
            for(int x = 0; x < ChunkSize; x++)
            {
                for(int y = 0; y < ChunkSize; y++)
                {
                    for(int z = 0; z < ChunkSize; z++)
                    {
                        try
                        {
                            this.data.Merge(this.blocks[x, y, z].Draw(x, y, z, this.blocks, this.Pos));
                        }
                        catch(Exception e)
                        {
                            Debug.Log($@"{GameManager.Time}: Can't Update Chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}: {e}");
                            Logger.Log($@"{GameManager.Time}: Can't Update Chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}: {e}");
                            this.drawLock = false;
                            break;
                        }
                    }
                }
            }
            this.drawLock = false;
            this.hasDrawn = true;
        }

        private void ApplyMesh()
        {
            this.renderingLock = true;
            Mesh mesh = this.data.ToMesh();
            if(this.GO == null)
            {
                this.GO = new GameObject
                {
                    name = $@"C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}"
                };
            }
            Transform cTransform = this.GO.transform;
            if(cTransform.gameObject.GetComponent<MeshFilter>() == null)
            {
                cTransform.gameObject.AddComponent<MeshFilter>();
                cTransform.gameObject.AddComponent<MeshRenderer>();
                cTransform.gameObject.AddComponent<MeshCollider>();
                cTransform.transform.position = new Vector3(this.Pos.x * ChunkSize, this.Pos.y * ChunkSize, this.Pos.z * ChunkSize);
                Texture2D atlas = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                atlas.LoadImage(System.IO.File.ReadAllBytes("Assets/Resources/Textures/Atlas/atlas.png"));
                atlas.filterMode = FilterMode.Point;
                atlas.wrapMode = TextureWrapMode.Clamp;
                cTransform.gameObject.GetComponent<MeshRenderer>().material.mainTexture = atlas;
                cTransform.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
            }
            cTransform.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            cTransform.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            this.GO.isStatic = true;
            this.renderingLock = false;
            this.hasRendered = true;
        }

        // Get noise
        private float GetNoise(Int3 _pos)
        {
            return (float)this.biome.Perlin.GetValue(_pos.x, _pos.y, _pos.z) + ((_pos.y - (128 * 0.3f)) * this.biome.YMultiplier);
        }

        // Get noise for Cave Generation
        private float GetNoiseCave(Int3 _pos)
        {
            return (float)this.biome.Ridged.GetValue(_pos.x, _pos.y, _pos.z) - (_pos.y / (128 * 0.5f) * this.biome.CaveYMultiplier);
        }

        // Get noise for tree generation
        //private float GetNoiseForTree(float x, float z)
        //{
        //    x += this.Pos.x * ChunkSize;
        //    z += this.Pos.z * ChunkSize;
        //    float xz = Mathf.PerlinNoise(x / GameManager.Streedx, z / GameManager.Streedz) * GameManager.Streemul;
        //    float oxz = Mathf.PerlinNoise((x / GameManager.Streendx) + GameManager.Streeoffset, (z / GameManager.Streendz) - GameManager.Streeoffset) * GameManager.Streenmul;
        //    xz = (xz + oxz) / 2f;
        //    return xz;
        //}

        // Get Block, given Chunk Internal Coords as Int3
        public Block GetBlockFromChunkInternalCoords(Int3 _pos)
        {
            return this.blocks[_pos.x, _pos.y, _pos.z];
        }

        // Set Block at position called by player, given Int3 and Block
        public void SetBlockPlayer(Int3 _pos, Block _block)
        {
            this.blocks[_pos.x, _pos.y, _pos.z] = _block;
            this.NeedToUpdate = true;
            this.HasBeenModified = true;
            if(_pos.x == 0)
            {
                this.NeedToUpdateNegXNeighbor = true;
            }
            if(_pos.x == ChunkSize - 1)
            {
                this.NeedToUpdatePosXNeighbor = true;
            }
            if(_pos.y == 0)
            {
                this.NeedToUpdateNegYNeighbor = true;
            }
            if(_pos.y == ChunkSize - 1)
            {
                this.NeedToUpdatePosYNeighbor = true;
            }
            if(_pos.z == 0)
            {
                this.NeedToUpdateNegZNeighbor = true;
            }
            if(_pos.z == ChunkSize - 1)
            {
                this.NeedToUpdatePosZNeighbor = true;
            }
        }

        // Set Block at position called by Structure Generator, given int x, y, z and Block
        private void SetBlockStructure(Int3 _pos, Block _block)
        {
            this.blocks[_pos.x, _pos.y, _pos.z] = _block;
        }

        // Degenerate Chunk
        public void Degenerate()
        {
            // First: save unloading chunks to file
            try
            {
                // Only save chunks that have been modified by player to save disk space of save files
                if(this.HasBeenModified)
                {
                    World.TaskFactory.StartNew(() => Serializer.SerializeToFile(FileManager.GetChunkString(this.Pos), this.GetChunkSaveData()));
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
            // Second: Destroy Chunk GameObject
            GameManager.Instance.RegisterDelegate(new Action(() =>
            {
                UnityEngine.Object.Destroy(this.GO);
            }));
            // Third: Remove Chunk from World
            World.Instance.AddChunkToRemoveList(this.Pos);
            //Debug.Log($@"{GameManager.time}: Degenerated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
            //Logger.Log($@"{GameManager.time}: Degenerated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
        }

        // Get ChunkData as array of ints
        public int[,,] GetChunkSaveData()
        {
            return this.blocks.ToIntArray();
        }

        // Get ChunkData as array of Blocks
        public void LoadChunkFromData(int[,,] _data)
        {
            this.blocks = _data.ToBlockArray();
        }

        // Get highest clear block: for tree generation
        //public static int GetHighestClearBlockPositionTree(Block[,,] blocks, int x, int z)
        //{
        //    int y = Chunk.ChunkSize - 1;
        //    for(int i = Chunk.ChunkSize - 7; i >= 1; i--)
        //    {
        //        if(blocks[x, i, z].IsTransparent && blocks[x, i + 1, z].IsTransparent && blocks[x, i + 2, z].IsTransparent
        //            && blocks[x, i + 3, z].IsTransparent && blocks[x, i + 4, z].IsTransparent && blocks[x, i + 5, z].IsTransparent
        //            && blocks[x, i + 6, z].IsTransparent && !blocks[x, i - 1, z].IsTransparent && !blocks[x, i - 1, z].Equals(Block.Leaves) && !blocks[x, i - 1, z].Equals(Block.Logs))
        //        {
        //            y = i;
        //            return y;
        //        }
        //        if(!blocks[x, i, z].IsTransparent && !blocks[x, i + 1, z].IsTransparent)
        //        {
        //            break;
        //        }
        //    }
        //    return 0;
        //}

        // Generate tree at position
        //public static void GenerateTree(Block[,,] blocks, int x, int y, int z, Int3 ChunkPos)
        //{
        //    Chunk currentchunk;
        //    currentchunk = World.Instance.GetChunk(ChunkPos);
        //    if(x - 2 > 0 && x + 2 < Chunk.ChunkSize - 1 && z - 2 > 0 && z + 2 < Chunk.ChunkSize - 1)
        //    {
        //        currentchunk.StructureSetBlock(x, y, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x, y + 1, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x, y + 2, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x, y + 3, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x, y + 4, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x, y + 5, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x - 1, y + 5, z, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 1, y + 5, z, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 5, z - 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 5, z + 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 1, y + 5, z - 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 1, y + 5, z + 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 1, y + 5, z - 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 1, y + 5, z + 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 2, y + 5, z, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 2, y + 5, z, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 5, z - 2, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 5, z + 2, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 2, y + 5, z - 2, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 2, y + 5, z + 2, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 2, y + 5, z - 2, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 2, y + 5, z + 2, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 6, z, Block.Logs);
        //        currentchunk.StructureSetBlock(x - 1, y + 6, z, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 1, y + 6, z, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 6, z - 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x, y + 6, z + 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 1, y + 6, z - 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x - 1, y + 6, z + 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 1, y + 6, z - 1, Block.Leaves);
        //        currentchunk.StructureSetBlock(x + 1, y + 6, z + 1, Block.Leaves);
        //    }
        //}
    }

}
