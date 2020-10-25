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
    /// <param name="block">The block to create mesh for.</param>
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
        // Top Face
        if(topVis)
        {
            string uvName = ((block.UniqueFaces & Block.UniqueFacesEnum.Top) == Block.UniqueFacesEnum.Top) ? block.BlockName + "_" + block.TopTextureName : block.BlockName;
            // TODO: When rotating UVs randomly grab the value from the noise map for block generation instead and get rid of the Block.RandomNumber field.
            Vector2[] uvs = ((block.RandomFaces & Block.RandomFacesEnum.Top) == Block.RandomFacesEnum.Top) ? UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber) : UVMap.GetUVs(uvName);
            meshData.Merge(new MeshData(topFaceVerts, topFaceTriangles, uvs, defaultUVs));
        }
        // Bottom Face
        if(bottomVis)
        {
            string uvName = ((block.UniqueFaces & Block.UniqueFacesEnum.Bottom) == Block.UniqueFacesEnum.Bottom) ?  block.BlockName + "_" + block.BottomTextureName : block.BlockName;
            Vector2[] uvs = ((block.RandomFaces & Block.RandomFacesEnum.Bottom) == Block.RandomFacesEnum.Bottom) ? UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber) : UVMap.GetUVs(uvName);
            meshData.Merge(new MeshData(bottomFaceVerts, bottomFaceTriangles, uvs, defaultUVs));
        }
        // Front Face
        if(frontVis)
        {
            if(block.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(0, -1, 1), out Block neighborBlock) == true && neighborBlock.BlockName == "Grass")
            {
                string uvName = block.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber);
                meshData.Merge(new MeshData(frontFaceVerts, frontFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((block.UniqueFaces & Block.UniqueFacesEnum.Front) == Block.UniqueFacesEnum.Front) ? block.BlockName + "_" + block.FrontTextureName : block.BlockName;
                Vector2[] uvs = ((block.RandomFaces & Block.RandomFacesEnum.Front) == Block.RandomFacesEnum.Front) ? UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(frontFaceVerts, frontFaceTriangles, uvs, defaultUVs));
            }
        }
        // Back Face
        if(backVis)
        {
            if(block.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(0, -1, -1), out Block neighborBlock) == true && neighborBlock.BlockName == "Grass")
            {
                string uvName = block.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber);
                meshData.Merge(new MeshData(backFaceVerts, backFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((block.UniqueFaces & Block.UniqueFacesEnum.Back) == Block.UniqueFacesEnum.Back) ? block.BlockName + "_" + block.BackTextureName : block.BlockName;
                Vector2[] uvs = ((block.RandomFaces & Block.RandomFacesEnum.Back) == Block.RandomFacesEnum.Back) ? UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(backFaceVerts, backFaceTriangles, uvs, defaultUVs));
            }
        }
        // Right Face
        if(rightVis)
        {
            if(block.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(1, -1, 0), out Block neighborBlock) == true && neighborBlock.BlockName == "Grass")
            {
                string uvName = block.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber);
                meshData.Merge(new MeshData(rightFaceVerts, rightFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((block.UniqueFaces & Block.UniqueFacesEnum.Right) == Block.UniqueFacesEnum.Right) ? block.BlockName + "_" + block.RightTextureName : block.BlockName;
                Vector2[] uvs = ((block.RandomFaces & Block.RandomFacesEnum.Right) == Block.RandomFacesEnum.Right) ? UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber) : UVMap.GetUVs(uvName).RotateUVs(3);
                meshData.Merge(new MeshData(rightFaceVerts, rightFaceTriangles, uvs, defaultUVs));
            }
        }
        // Left Face
        if(leftVis)
        {
            if(block.BlockName == "Grass" && World.TryGetBlockFromWorldPos(internalPos.InternalPosToWorldPos(chunkPos) + new Vector3Int(-1, -1, 0), out Block neighborBlock) == true && neighborBlock.BlockName == "Grass")
            {
                string uvName = block.BlockName;
                Vector2[] uvs = UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber);
                meshData.Merge(new MeshData(leftFaceVerts, leftFaceTriangles, uvs, defaultUVs));
            }
            else
            {
                string uvName = ((block.UniqueFaces & Block.UniqueFacesEnum.Left) == Block.UniqueFacesEnum.Left) ? block.BlockName + "_" + block.LeftTextureName : block.BlockName;
                Vector2[] uvs = ((block.RandomFaces & Block.RandomFacesEnum.Left) == Block.RandomFacesEnum.Left) ? UVMap.GetUVs(uvName).RotateUVs(block.RandomNumber) : UVMap.GetUVs(uvName).RotateUVs(3);
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
            if(internalPos.y < GameManager.Instance.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 1, 0));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.y == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int topNeighborWorldPos = worldPos;
                topNeighborWorldPos += new Vector3Int(0, 1, 0);
                if(World.TryGetBlockFromWorldPos(topNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return true;
            }
        }
        else if(side == Side.Bottom)
        {
            if(internalPos.y > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, -1, 0));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.y == 0
            {
                Vector3Int bottomNeighborWorldPos = worldPos;
                bottomNeighborWorldPos += new Vector3Int(0, -1, 0);
                if(World.TryGetBlockFromWorldPos(bottomNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Front)
        {
            if(internalPos.z < GameManager.Instance.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 0, 1));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.z == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int frontNeighborWorldPos = worldPos;
                frontNeighborWorldPos += new Vector3Int(0, 0, 1);
                if(World.TryGetBlockFromWorldPos(frontNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Back)
        {
            if(internalPos.z > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 0, -1));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.z == 0
            {
                Vector3Int backNeighborWorldPos = worldPos;
                backNeighborWorldPos += new Vector3Int(0, 0, -1);
                if(World.TryGetBlockFromWorldPos(backNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Right)
        {
            if(internalPos.x < GameManager.Instance.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(1, 0, 0));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.x == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int rightNeighborWorldPos = worldPos;
                rightNeighborWorldPos += new Vector3Int(1, 0, 0);
                if(World.TryGetBlockFromWorldPos(rightNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Left)
        {
            if(internalPos.x > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(-1, 0, 0));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else// internalPos.x == 0
            {
                Vector3Int leftNeighborWorldPos = worldPos;
                leftNeighborWorldPos += new Vector3Int(-1, 0, 0);
                if(World.TryGetBlockFromWorldPos(leftNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
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
