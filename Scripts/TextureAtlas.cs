using System.IO;

using UnityEngine;

// Class containg TextureAtlas creation
public class TextureAtlas
{
    // TextureAtlas fields
    public static readonly TextureAtlas Instance = new TextureAtlas();

    // Atlas getter
    public static Texture2D Atlas { get; private set; }

    // Creates Texture Atlas from all textures in /Blocks/ FOLDER
    public void CreateAtlas()
    {
        string[] images = Directory.GetFiles("Assets/Resources/Textures/Blocks/", "*.png");
        int pixelWidth = 32;
        int pixelHeight = 32;
        int atlasWidth = Mathf.CeilToInt((Mathf.Sqrt(images.Length) + 1) * pixelWidth);
        int atlasHeight = Mathf.CeilToInt((Mathf.Sqrt(images.Length) + 1) * pixelHeight);
        Texture2D atlas = new Texture2D(atlasWidth, atlasHeight);
        int count = 0;
        for(int x = 0; x < atlasWidth / pixelWidth; x++)
        {
            for(int y = 0; y < atlasHeight / pixelHeight; y++)
            {
                if(count > images.Length - 1)
                {
                    goto end;
                }
                Texture2D temp = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                temp.LoadImage(File.ReadAllBytes(images[count]));
                atlas.SetPixels(x * pixelWidth, y * pixelHeight, pixelWidth, pixelHeight, temp.GetPixels());
                float startx = x * pixelWidth;
                float starty = y * pixelHeight;
                float perpixelratiox = 1.0f / (float)atlas.width;
                float perpixelratioy = 1.0f / (float)atlas.height;
                startx *= perpixelratiox;
                starty *= perpixelratioy;
                float endx = startx + (perpixelratiox * pixelWidth);
                float endy = starty + (perpixelratioy * pixelHeight);
                UVMap map = new UVMap(images[count], new Vector2[]
                {
                    new Vector2(startx, starty),
                    new Vector2(startx, endy),
                    new Vector2(endx, starty),
                    new Vector2(endx, endy)
                });
                map.Register();
                count++;
            }
        }
        end:;
        Atlas = atlas;
        File.WriteAllBytes("Assets/Resources/Textures/Atlas/atlas.png", atlas.EncodeToPNG());
    }
}
