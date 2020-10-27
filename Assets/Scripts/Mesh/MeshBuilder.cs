using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Class for creating meshes for chunks on a block by block basis.
/// </summary>
public static class MeshBuilder
{
    // List of Face Vertices and Triangle Draw Orders
    private static readonly List<Vector3> topFaceVerts = new List<Vector3>()
    {
        new Vector3(0,1,0),
        new Vector3(0,1,1),
        new Vector3(1,1,0),
        new Vector3(1,1,1)
    };
    private static readonly List<int> topFaceTriangles = new List<int>()
    {
        0,1,2,3,2,1
    };
    private static readonly List<Vector3> bottomFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        new Vector3(1,0,0),
        new Vector3(1,0,1)
    };
    private static readonly List<int> bottomFaceTriangles = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> frontFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(0,1,1),
        new Vector3(1,1,1)
    };
    private static readonly List<int> frontFaceTriangles = new List<int>()
    {
        0,1,2,3,2,1
    };
    private static readonly List<Vector3> backFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(1,1,0)
    };
    private static readonly List<int> backFaceTriangles = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> rightFaceVerts = new List<Vector3>()
    {
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(1,1,0),
        new Vector3(1,1,1)
    };
    private static readonly List<int> rightFaceTriangles = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> leftFaceVerts = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(0,1,1)
    };
    private static readonly List<int> leftFaceTriangles = new List<int>()
    {
        0,1,2,3,2,1
    };

    private static readonly Vector2[] defaultUVs = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1)
    };
    // End of list of Face Vertices and Triangle Draw Orders

    private enum Side
    {
        Top,
        Bottom,
        Front,
        Back,
        Right,
        Left,
    };

    /// <summary>
    /// Creates the mesh data for the block at the given internal position within the chunk at the given chunk position.
    /// </summary>
    /// <param name="block">The block type to create mesh for.</param>
    /// <param name="internalPos">The internal position of the block to create a mesh for.</param>
    /// <param name="chunkPos">The chunk position of the block to create a mesh for.</param>
    /// <returns>Returns the mesh data created for the block.</returns>
    public static MeshData CreateMesh(Block block, Vector3Int internalPos, Vector3Int chunkPos)
    {
        // TODO: Add support for block rotation so blocks with different faces can be placed in any orientation, or at least additional orientations.
        // TODO: Add lighting data into UV2 for mesh and use it in shader to determine brightness of texture.
        MeshData meshData = new MeshData();
        bool topVis = CheckFaceVisibility(internalPos, chunkPos, Side.Top, out _);
        bool bottomVis = CheckFaceVisibility(internalPos, chunkPos, Side.Bottom, out _);
        bool frontVis = CheckFaceVisibility(internalPos, chunkPos, Side.Front, out _);
        bool backVis = CheckFaceVisibility(internalPos, chunkPos, Side.Back, out _);
        bool rightVis = CheckFaceVisibility(internalPos, chunkPos, Side.Right, out _);
        bool leftVis = CheckFaceVisibility(internalPos, chunkPos, Side.Left, out _);
        Vector3Int worldPos = internalPos.InternalPosToWorldPos(chunkPos);
        int rotation = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 3));
        BlockType blockType = BlockType.BlockTypes[block.ID];
        // Top Face
        if(topVis)
        {
            string uvName = ((blockType.UniqueFaces & BlockType.UniqueFacesEnum.Top) == BlockType.UniqueFacesEnum.Top) ? blockType.BlockName + "_" + blockType.TopTextureName : blockType.BlockName;
            Vector2[] uvs = ((blockType.RandomFaces & BlockType.RandomFacesEnum.Top) == BlockType.RandomFacesEnum.Top) ? UVMap.GetUVs(uvName).RotateUVs(rotation) : UVMap.GetUVs(uvName);
            meshData.Merge(new MeshData(topFaceVerts, topFaceTriangles, uvs, defaultUVs));
        }
        // Bottom Face
        if(bottomVis)
        {
            string uvName = ((blockType.UniqueFaces & BlockType.UniqueFacesEnum.Bottom) == BlockType.UniqueFacesEnum.Bottom) ?  blockType.BlockName + "_" + blockType.BottomTextureName : blockType.BlockName;
            Vector2[] uvs = ((blockType.RandomFaces & BlockType.RandomFacesEnum.Bottom) == BlockType.RandomFacesEnum.Bottom) ? UVMap.GetUVs(uvName).RotateUVs(rotation) : UVMap.GetUVs(uvName);
            meshData.Merge(new MeshData(bottomFaceVerts, bottomFaceTriangles, uvs, defaultUVs));
        }
        // Front Face
        if(frontVis)
        {
            if(blockType.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(0, -1, 1), out Block neighborBlock) == true && BlockType.BlockTypes[neighborBlock.ID].BlockName == "Grass")
            {
                string uvName = blockType.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(rotation);
                meshData.Merge(new MeshData(frontFaceVerts, frontFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((blockType.UniqueFaces & BlockType.UniqueFacesEnum.Front) == BlockType.UniqueFacesEnum.Front) ? blockType.BlockName + "_" + blockType.FrontTextureName : blockType.BlockName;
                Vector2[] uvs = ((blockType.RandomFaces & BlockType.RandomFacesEnum.Front) == BlockType.RandomFacesEnum.Front) ? UVMap.GetUVs(uvName).RotateUVs(rotation) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(frontFaceVerts, frontFaceTriangles, uvs, defaultUVs));
            }
        }
        // Back Face
        if(backVis)
        {
            if(blockType.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(0, -1, -1), out Block neighborBlock) == true && BlockType.BlockTypes[neighborBlock.ID].BlockName == "Grass")
            {
                string uvName = blockType.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(rotation);
                meshData.Merge(new MeshData(backFaceVerts, backFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((blockType.UniqueFaces & BlockType.UniqueFacesEnum.Back) == BlockType.UniqueFacesEnum.Back) ? blockType.BlockName + "_" + blockType.BackTextureName : blockType.BlockName;
                Vector2[] uvs = ((blockType.RandomFaces & BlockType.RandomFacesEnum.Back) == BlockType.RandomFacesEnum.Back) ? UVMap.GetUVs(uvName).RotateUVs(rotation) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(backFaceVerts, backFaceTriangles, uvs, defaultUVs));
            }
        }
        // Right Face
        if(rightVis)
        {
            if(blockType.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(1, -1, 0), out Block neighborBlock) == true && BlockType.BlockTypes[neighborBlock.ID].BlockName == "Grass")
            {
                string uvName = blockType.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(rotation);
                meshData.Merge(new MeshData(rightFaceVerts, rightFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((blockType.UniqueFaces & BlockType.UniqueFacesEnum.Right) == BlockType.UniqueFacesEnum.Right) ? blockType.BlockName + "_" + blockType.RightTextureName : blockType.BlockName;
                Vector2[] uvs = ((blockType.RandomFaces & BlockType.RandomFacesEnum.Right) == BlockType.RandomFacesEnum.Right) ? UVMap.GetUVs(uvName).RotateUVs(rotation) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(rightFaceVerts, rightFaceTriangles, uvs, defaultUVs));
            }
        }
        // Left Face
        if(leftVis)
        {
            if(blockType.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(-1, -1, 0), out Block neighborBlock) == true && BlockType.BlockTypes[neighborBlock.ID].BlockName == "Grass")
            {
                string uvName = blockType.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(rotation);
                meshData.Merge(new MeshData(leftFaceVerts, leftFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((blockType.UniqueFaces & BlockType.UniqueFacesEnum.Left) == BlockType.UniqueFacesEnum.Left) ? blockType.BlockName + "_" + blockType.LeftTextureName : blockType.BlockName;
                Vector2[] uvs = ((blockType.RandomFaces & BlockType.RandomFacesEnum.Left) == BlockType.RandomFacesEnum.Left) ? UVMap.GetUVs(uvName).RotateUVs(rotation) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(leftFaceVerts, leftFaceTriangles, uvs, defaultUVs));
            }
        }
        meshData.OffsetPosition(new Vector3(internalPos.x - 0.5f, internalPos.y - 0.5f, internalPos.z - 0.5f));
        return meshData;
    }

    /// <summary>
    /// Checks if the specified face of the block at the given position is visible.
    /// </summary>
    /// <param name="internalPos">The internal position of the block to check face visibility for.</param>
    /// <param name="chunkPos">The chunk position of the block to check face visibility for.</param>
    /// <param name="side">The side of the block to check face visibility for.</param>
    /// <returns>Returns true if that face is visible.</returns>
    private static bool CheckFaceVisibility(Vector3Int internalPos, Vector3Int chunkPos, Side side, out int lightingValue)
    {
        // TODO: When getting face visibility, if face is visible also get lighting values from nearby block.
        lightingValue = 0;
        Vector3Int worldPos = internalPos.InternalPosToWorldPos(chunkPos);
        if(World.TryGetChunk(chunkPos, out Chunk chunk) == false)
        {
            return false;
        }
        if(side == Side.Top)
        {
            if(internalPos.y < GameManager.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 1, 0));
                BlockType blockType = BlockType.BlockTypes[block.ID];
                return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.y == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int topNeighborWorldPos = worldPos;
                topNeighborWorldPos += new Vector3Int(0, 1, 0);
                if(World.TryGetBlockFromWorldPos(topNeighborWorldPos, out Block block) == true)
                {
                    BlockType blockType = BlockType.BlockTypes[block.ID];
                    return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
                }
                return true;
            }
        }
        else if(side == Side.Bottom)
        {
            if(internalPos.y > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, -1, 0));
                BlockType blockType = BlockType.BlockTypes[block.ID];
                return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.y == 0
            {
                Vector3Int bottomNeighborWorldPos = worldPos;
                bottomNeighborWorldPos += new Vector3Int(0, -1, 0);
                if(World.TryGetBlockFromWorldPos(bottomNeighborWorldPos, out Block block) == true)
                {
                    BlockType blockType = BlockType.BlockTypes[block.ID];
                    return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Front)
        {
            if(internalPos.z < GameManager.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 0, 1));
                BlockType blockType = BlockType.BlockTypes[block.ID];
                return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.z == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int frontNeighborWorldPos = worldPos;
                frontNeighborWorldPos += new Vector3Int(0, 0, 1);
                if(World.TryGetBlockFromWorldPos(frontNeighborWorldPos, out Block block) == true)
                {
                    BlockType blockType = BlockType.BlockTypes[block.ID];
                    return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Back)
        {
            if(internalPos.z > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 0, -1));
                BlockType blockType = BlockType.BlockTypes[block.ID];
                return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.z == 0
            {
                Vector3Int backNeighborWorldPos = worldPos;
                backNeighborWorldPos += new Vector3Int(0, 0, -1);
                if(World.TryGetBlockFromWorldPos(backNeighborWorldPos, out Block block) == true)
                {
                    BlockType blockType = BlockType.BlockTypes[block.ID];
                    return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Right)
        {
            if(internalPos.x < GameManager.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(1, 0, 0));
                BlockType blockType = BlockType.BlockTypes[block.ID];
                return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.x == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int rightNeighborWorldPos = worldPos;
                rightNeighborWorldPos += new Vector3Int(1, 0, 0);
                if(World.TryGetBlockFromWorldPos(rightNeighborWorldPos, out Block block) == true)
                {
                    BlockType blockType = BlockType.BlockTypes[block.ID];
                    return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Left)
        {
            if(internalPos.x > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(-1, 0, 0));
                BlockType blockType = BlockType.BlockTypes[block.ID];
                return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
            }
            else// internalPos.x == 0
            {
                Vector3Int leftNeighborWorldPos = worldPos;
                leftNeighborWorldPos += new Vector3Int(-1, 0, 0);
                if(World.TryGetBlockFromWorldPos(leftNeighborWorldPos, out Block block) == true)
                {
                    BlockType blockType = BlockType.BlockTypes[block.ID];
                    return blockType.Transparency == BlockType.TransparencyEnum.Transparent || blockType.Transparency == BlockType.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
