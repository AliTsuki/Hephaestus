using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Block : ushort
{
    Air = 0x0000,
    Stone = 0x0001,
    Grass = 0x0002,
    Dirt = 0x0003
}

public static class Block_Extensions
{
    public static bool IsTransparent(this Block block)
    {
        return block == Block.Air;
    }
}
