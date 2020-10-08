using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Class for creating meshes for chunks on a block by block basis.
/// </summary>
public static class MeshBuilder
{
    // List of Face Vertices and Triangle Draw Orders
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
    private static readonly List<Vector3> frontFaceVerts = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(0,1,1)
    };
    private static readonly List<int> frontFaceTriangles = new List<int>()
    {
        0,1,2,3,2,1
    };
    private static readonly List<Vector3> backFaceVerts = new List<Vector3>()
    {
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(1,1,0),
        new Vector3(1,1,1)
    };
    private static readonly List<int> backFaceTriangles = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> leftFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(1,1,0)
    };
    private static readonly List<int> leftFaceTriangles = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> rightFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(0,1,1),
        new Vector3(1,1,1)
    };
    private static readonly List<int> rightFaceTriangles = new List<int>()
    {
        0,1,2,3,2,1
    };
    // End of list of Face Vertices and Triangle Draw Orders
   
    private enum Side
    {
        Bottom,
        Top,
        Front,
        Back,
        Left,
        Right,
    };

    /// <summary>
    /// Creates the mesh data for the block at the given internal position within the chunk at the given chunk position.
    /// </summary>
    /// <param name="internalPos">The internal position of the block to create a mesh for.</param>
    /// <param name="chunkPos">The chunk position of the block to create a mesh for.</param>
    /// <returns>Returns the mesh data created for the block.</returns>
    public static MeshData DrawCube(Vector3Int internalPos, Vector3Int chunkPos)
    {
        MeshData meshData = new MeshData();
        bool bottomVis = CheckFaceVisibility(internalPos, chunkPos, Side.Bottom);
        bool topVis = CheckFaceVisibility(internalPos, chunkPos, Side.Top);
        bool frontVis = CheckFaceVisibility(internalPos, chunkPos, Side.Front);
        bool backVis = CheckFaceVisibility(internalPos, chunkPos, Side.Back);
        bool leftVis = CheckFaceVisibility(internalPos, chunkPos, Side.Left);
        bool rightVis = CheckFaceVisibility(internalPos, chunkPos, Side.Right);
        // Bottom Face
        if(bottomVis)
        {
            meshData.Merge(new MeshData(
            bottomFaceVerts,
            bottomFaceTriangles,
            new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            }));
        }
        // Top Face
        if(topVis)
        {
            meshData.Merge(new MeshData(
            topFaceVerts,
            topFaceTriangles,
            new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            }));
        }
        // Front Face
        if(frontVis)
        {
            meshData.Merge(new MeshData(
            frontFaceVerts,
            frontFaceTriangles,
            new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            }));
        }
        // Back Face
        if(backVis)
        {
            meshData.Merge(new MeshData(
            backFaceVerts,
            backFaceTriangles,
            new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            }));
        }
        // Left Face
        if(leftVis)
        {
            meshData.Merge(new MeshData(
            leftFaceVerts,
            leftFaceTriangles,
            new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            }));
        }
        // Right Face
        if(rightVis)
        {
            meshData.Merge(new MeshData(
            rightFaceVerts,
            rightFaceTriangles,
            new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            }));
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
    private static bool CheckFaceVisibility(Vector3Int internalPos, Vector3Int chunkPos, Side side)
    {
        Vector3Int worldPos = internalPos.InternalPosToWorldPos(chunkPos);
        if(World.TryGetChunk(chunkPos, out Chunk chunk) == false)
        {
            return false;
        }
        if(side == Side.Bottom)
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
        else if(side == Side.Top)
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
                return false;
            }
        }
        else if(side == Side.Front)
        {
            if(internalPos.x > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(-1, 0, 0));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.x == 0
            {
                Vector3Int frontNeighborWorldPos = worldPos;
                frontNeighborWorldPos += new Vector3Int(-1, 0, 0);
                if(World.TryGetBlockFromWorldPos(frontNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Back)
        {
            if(internalPos.x < GameManager.Instance.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(1, 0, 0));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.x == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int backNeighborWorldPos = worldPos;
                backNeighborWorldPos += new Vector3Int(1, 0, 0);
                if(World.TryGetBlockFromWorldPos(backNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Left)
        {
            if(internalPos.z > 0)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 0, -1));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else// internalPos.z == 0
            {
                Vector3Int leftNeighborWorldPos = worldPos;
                leftNeighborWorldPos += new Vector3Int(0, 0, -1);
                if(World.TryGetBlockFromWorldPos(leftNeighborWorldPos, out Block block))
                {
                    return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
                }
                return false;
            }
        }
        else if(side == Side.Right)
        {
            if(internalPos.z < GameManager.Instance.ChunkSize - 1)
            {
                Block block = chunk.GetBlock(internalPos + new Vector3Int(0, 0, 1));
                return block.Transparency == Block.TransparencyEnum.Transparent || block.Transparency == Block.TransparencyEnum.SemiTransparent;
            }
            else // internalPos.z == GameManager.Instance.ChunkSize - 1
            {
                Vector3Int rightNeighborWorldPos = worldPos;
                rightNeighborWorldPos += new Vector3Int(0, 0, 1);
                if(World.TryGetBlockFromWorldPos(rightNeighborWorldPos, out Block block))
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
