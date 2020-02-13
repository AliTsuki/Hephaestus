using System.Collections.Generic;

using UnityEngine;

// Class for Mesh maths
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
    private static readonly List<int> bottomFaceDrawOrder = new List<int>()
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
    private static readonly List<int> topFaceDrawOrder = new List<int>()
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
    private static readonly List<int> frontFaceDrawOrder = new List<int>()
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
    private static readonly List<int> backFaceDrawOrder = new List<int>()
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
    private static readonly List<int> leftFaceDrawOrder = new List<int>()
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
    private static readonly List<int> rightFaceDrawOrder = new List<int>()
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

    // Create Mesh for Cube, given Chunk Internal Coords as int(x,y,z), Block 3D Array blocks, Block block, UVMaps as Vector2 Arrays, and Chunk Coords as int(PosX, PosY, PosZ)
    // Draw Grass Block
    public static MeshData DrawCube(int _x, int _y, int _z, Block[,,] _blocks, Block _block, Block.BlockFaceStyle _style, Vector2[] _uvMap0, Vector2[] _uvMap1, Vector2[] _uvMap2, Vector2[] _uvMap3, Vector2[] _uvMap4, Vector2[] _uvMap5, Int3 _chunkPos)
    {
        MeshData data = new MeshData();
        Int3 pos = new Int3(_x, _y, _z);
        pos.ChunkInternalCoordsToWorldCoords(_chunkPos);
        // Bottom Face
        bool bottomVis = CheckVis(_x, _y, _z, pos, _blocks, Side.Bottom);
        bool topVis = CheckVis(_x, _y, _z, pos, _blocks, Side.Top);
        bool frontVis = CheckVis(_x, _y, _z, pos, _blocks, Side.Front);
        bool backVis = CheckVis(_x, _y, _z, pos, _blocks, Side.Back);
        bool leftVis = CheckVis(_x, _y, _z, pos, _blocks, Side.Left);
        bool rightVis = CheckVis(_x, _y, _z, pos, _blocks, Side.Right);
        if(bottomVis)
        {
            if(_style == Block.BlockFaceStyle.GrassStyle)
            {
                data.Merge(new MeshData(
                bottomFaceVerts,
                bottomFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap3[0],
                    _uvMap3[1],
                    _uvMap3[2],
                    _uvMap3[3]
                }));
            }
            else if(_style == Block.BlockFaceStyle.LogStyle)
            {
                data.Merge(new MeshData(
                bottomFaceVerts,
                bottomFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[0],
                    _uvMap0[1],
                    _uvMap0[2],
                    _uvMap0[3]
                }));
            }
            else if(_style == Block.BlockFaceStyle.UniversalStyle)
            {
                data.Merge(new MeshData(
                bottomFaceVerts,
                bottomFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[0],
                    _uvMap0[1],
                    _uvMap0[2],
                    _uvMap0[3]
                }));
            }
        }
        // Top Face
        if(topVis)
        {
            if(_style == Block.BlockFaceStyle.GrassStyle)
            {
                data.Merge(new MeshData(
                topFaceVerts,
                topFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[0],
                    _uvMap0[1],
                    _uvMap0[2],
                    _uvMap0[3]
                }));
            }
            else if(_style == Block.BlockFaceStyle.LogStyle)
            {
                data.Merge(new MeshData(
                topFaceVerts,
                topFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[0],
                    _uvMap0[1],
                    _uvMap0[2],
                    _uvMap0[3]
                }));
            }
            else if(_style == Block.BlockFaceStyle.UniversalStyle)
            {
                data.Merge(new MeshData(
                topFaceVerts,
                topFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[0],
                    _uvMap0[1],
                    _uvMap0[2],
                    _uvMap0[3]
                }));
            }
        }
        // Front Face
        if(frontVis)
        {
            if(_style == Block.BlockFaceStyle.GrassStyle)
            {
                if(topVis)
                {
                    data.Merge(new MeshData(
                    frontFaceVerts,
                    frontFaceDrawOrder,
                    new Vector2[]
                    {
                        _uvMap1[2],
                        _uvMap1[0],
                        _uvMap1[3],
                        _uvMap1[1]
                    }));
                }
                else
                {
                    data.Merge(new MeshData(
                    frontFaceVerts,
                    frontFaceDrawOrder,
                    new Vector2[]
                    {
                        _uvMap3[2],
                        _uvMap3[0],
                        _uvMap3[3],
                        _uvMap3[1]
                    }));
                }
            }
            else if(_style == Block.BlockFaceStyle.LogStyle)
            {
                data.Merge(new MeshData(
                frontFaceVerts,
                frontFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                }));
            }
            else if(_style == Block.BlockFaceStyle.UniversalStyle)
            {
                data.Merge(new MeshData(
                frontFaceVerts,
                frontFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[2],
                    _uvMap0[0],
                    _uvMap0[3],
                    _uvMap0[1]
                }));
            }
        }
        // Back Face
        if(backVis)
        {
            if(_style == Block.BlockFaceStyle.GrassStyle)
            {
                if(topVis)
                {
                    data.Merge(new MeshData(
                    backFaceVerts,
                    backFaceDrawOrder,
                    new Vector2[]
                    {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                    }));
                }
                else
                {
                    data.Merge(new MeshData(
                    backFaceVerts,
                    backFaceDrawOrder,
                    new Vector2[]
                    {
                    _uvMap3[2],
                    _uvMap3[0],
                    _uvMap3[3],
                    _uvMap3[1]
                    }));
                }
            }
            else if(_style == Block.BlockFaceStyle.LogStyle)
            {
                data.Merge(new MeshData(
                backFaceVerts,
                backFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                }));
            }
            else if(_style == Block.BlockFaceStyle.UniversalStyle)
            {
                data.Merge(new MeshData(
                backFaceVerts,
                backFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[2],
                    _uvMap0[0],
                    _uvMap0[3],
                    _uvMap0[1]
                }));
            }
        }
        // Left Face
        if(leftVis)
        {
            if(_style == Block.BlockFaceStyle.GrassStyle)
            {
                if(topVis)
                {
                    data.Merge(new MeshData(
                    leftFaceVerts,
                    leftFaceDrawOrder,
                    new Vector2[]
                    {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                    }));
                }
                else
                {
                    data.Merge(new MeshData(
                    leftFaceVerts,
                    leftFaceDrawOrder,
                    new Vector2[]
                    {
                    _uvMap3[2],
                    _uvMap3[0],
                    _uvMap3[3],
                    _uvMap3[1]
                    }));
                }
            }
            else if(_style == Block.BlockFaceStyle.LogStyle)
            {
                data.Merge(new MeshData(
                leftFaceVerts,
                leftFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                }));
            }
            else if(_style == Block.BlockFaceStyle.UniversalStyle)
            {
                data.Merge(new MeshData(
                leftFaceVerts,
                leftFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[2],
                    _uvMap0[0],
                    _uvMap0[3],
                    _uvMap0[1]
                }));
            }
        }
        // Right Face
        if(rightVis)
        {
            if(_style == Block.BlockFaceStyle.GrassStyle)
            {
                if(topVis)
                {
                    data.Merge(new MeshData(
                    rightFaceVerts,
                    rightFaceDrawOrder,
                    new Vector2[]
                    {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                    }));
                }
                else
                {
                    data.Merge(new MeshData(
                    rightFaceVerts,
                    rightFaceDrawOrder,
                    new Vector2[]
                    {
                    _uvMap3[2],
                    _uvMap3[0],
                    _uvMap3[3],
                    _uvMap3[1]
                    }));
                }
            }
            else if(_style == Block.BlockFaceStyle.LogStyle)
            {
                data.Merge(new MeshData(
                rightFaceVerts,
                rightFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap1[2],
                    _uvMap1[0],
                    _uvMap1[3],
                    _uvMap1[1]
                }));
            }
            else if(_style == Block.BlockFaceStyle.UniversalStyle)
            {
                data.Merge(new MeshData(
                rightFaceVerts,
                rightFaceDrawOrder,
                new Vector2[]
                {
                    _uvMap0[2],
                    _uvMap0[0],
                    _uvMap0[3],
                    _uvMap0[1]
                }));
            }
        }
        data.AddPos(new Vector3(_x - 0.5f, _y - 0.5f, _z - 0.5f));
        return data;
    }

    // Checks visibility of given face
    private static bool CheckVis(int _x, int _y, int _z, Int3 _position, Block[,,] _blocks, Side _side)
    {
        if(_side == Side.Bottom)
        {
            if(_y > 0)
            {
                Block block = _blocks[_x, _y - 1, _z];
                return block.IsTransparent || block.IsSemiTransparent;
            }
            else if(_y == 0)
            {
                Int3 pos = _position;
                pos.AddPos(0, -1, 0);
                Block block = World.Instance.GetBlockFromWorldCoords(pos);
                return block.IsTransparent || block.IsSemiTransparent;
            }
            return false;
        }
        else if(_side == Side.Top)
        {
            if(_y < Chunk.ChunkSize - 1)
            {
                Block block = _blocks[_x, _y + 1, _z];
                return block.IsTransparent || block.IsSemiTransparent;
            }
            else if(_y == Chunk.ChunkSize - 1)
            {
                Int3 pos = _position;
                pos.AddPos(0, 1, 0);
                Block block = World.Instance.GetBlockFromWorldCoords(pos);
                return block.IsTransparent || block.IsSemiTransparent;
            }
            return false;
        }
        else if(_side == Side.Front)
        {
            if(_x > 0)
            {
                Block block = _blocks[_x - 1, _y, _z];
                return block.IsTransparent || block.IsSemiTransparent;
            }
            else if(_x == 0)
            {
                Int3 pos = _position;
                pos.AddPos(-1, 0, 0);
                Block block = World.Instance.GetBlockFromWorldCoords(pos);
                return block.IsTransparent || block.IsSemiTransparent;
            }
            return false;
        }
        else if(_side == Side.Back)
        {
            if(_x < Chunk.ChunkSize - 1)
            {
                Block block = _blocks[_x + 1, _y, _z];
                return block.IsTransparent || block.IsSemiTransparent;
            }
            else if(_x == Chunk.ChunkSize - 1)
            {
                Int3 pos = _position;
                pos.AddPos(1, 0, 0);
                Block block = World.Instance.GetBlockFromWorldCoords(pos);
                return block.IsTransparent || block.IsSemiTransparent;
            }
            return false;
        }
        else if(_side == Side.Left)
        {
            if(_z > 0)
            {
                Block block = _blocks[_x, _y, _z - 1];
                return block.IsTransparent || block.IsSemiTransparent;
            }
            else if(_z == 0)
            {
                Int3 pos = _position;
                pos.AddPos(0, 0, -1);
                Block block = World.Instance.GetBlockFromWorldCoords(pos);
                return block.IsTransparent || block.IsSemiTransparent;
            }
            return false;
        }
        else // Side.Right
        {
            if(_z < Chunk.ChunkSize - 1)
            {
                Block block = _blocks[_x, _y, _z + 1];
                return block.IsTransparent || block.IsSemiTransparent;
            }
            else if(_z == Chunk.ChunkSize - 1)
            {
                Int3 pos = _position;
                pos.AddPos(0, 0, 1);
                Block block = World.Instance.GetBlockFromWorldCoords(pos);
                return block.IsTransparent || block.IsSemiTransparent;
            }
            return false;
        }
    }
}
