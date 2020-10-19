using System.IO;

using UnityEngine;


/// <summary>
/// Class used to create a texture atlas from all block textures.
/// </summary>
public static class TextureAtlas
{
    /// <summary>
    /// The atlas texture of all block textures.
    /// </summary>
    public static Texture2D AtlasTexture;

    /// <summary>
    /// Creates an atlas of all block textures and sets up the UVMaps to point to the UVs of each texture.
    /// </summary>
    public static void CreateAtlas()
    {
        string[] texturePaths = Directory.GetFiles(GameManager.OpaqueBlockTexturePath, "*?.png");
        int textureResolutionPixels = 32;
        int atlasResolutionPixels = Mathf.CeilToInt((Mathf.Sqrt(texturePaths.Length) + 1) * textureResolutionPixels);
        AtlasTexture = new Texture2D(atlasResolutionPixels, atlasResolutionPixels, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        int x = 0;
        int y = 0;
        foreach(string texturePath in texturePaths)
        {
            Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            texture.LoadImage(File.ReadAllBytes(texturePath));
            AtlasTexture.SetPixels(x * textureResolutionPixels, y * textureResolutionPixels, textureResolutionPixels, textureResolutionPixels, texture.GetPixels());
            float startX = x * textureResolutionPixels;
            float startY = y * textureResolutionPixels;
            float perPixelRatio = 1.0f / AtlasTexture.width;
            startX *= perPixelRatio;
            startY *= perPixelRatio;
            float endX = startX + (perPixelRatio * textureResolutionPixels);
            float endY = startY + (perPixelRatio * textureResolutionPixels);
            string texName = texturePath.Substring(GameManager.OpaqueBlockTexturePath.Length);
            texName = texName.Substring(0, texName.Length - 4);
            _ = new UVMap(texName, new Vector2[]
            {
                    new Vector2(startX, startY),
                    new Vector2(startX, endY),
                    new Vector2(endX, startY),
                    new Vector2(endX, endY)
            });
            x++;
            if(x >= atlasResolutionPixels / textureResolutionPixels)
            {
                x = 0;
                y++;
            }
        }
        AtlasTexture.Apply();
        File.WriteAllBytes(GameManager.TextureAtlasPath, AtlasTexture.EncodeToPNG());
        GameManager.Instance.ChunkMaterial.SetTexture("_BaseColorMap", AtlasTexture);
    }
}
