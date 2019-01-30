using System.IO;

using UnityEngine;

// Class containg TextureAtlas creation
public class TextureAtlas
{
    // TextureAtlas objects
    public static readonly TextureAtlas Instance = new TextureAtlas();

    // Atlas getter
    public static Texture2D _ATLAS { get; private set; }

    // Creates Texture Atlas from all textures in /Blocks/ FOLDER
    public void CreateAtlas()
    {
        string[] _Images = Directory.GetFiles("Assets/Resources/Textures/Blocks/", "*.png");
        int PixelWidth = 32;
        int PixelHeight = 32;
        int atlaswidth = Mathf.CeilToInt((Mathf.Sqrt(_Images.Length) + 1) * PixelWidth);
        int atlasheight = Mathf.CeilToInt((Mathf.Sqrt(_Images.Length) + 1) * PixelHeight);
        Texture2D Atlas = new Texture2D(atlaswidth, atlasheight);
        int count = 0;
        for(int x = 0; x < atlaswidth / PixelWidth; x++)
        {
            for(int y = 0; y < atlasheight / PixelHeight; y++)
            {
                if(count > _Images.Length - 1)
                {
                    goto end;
                }
                Texture2D temp = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                temp.LoadImage(File.ReadAllBytes(_Images[count]));
                Atlas.SetPixels(x * PixelWidth, y * PixelHeight, PixelWidth, PixelHeight, temp.GetPixels());
                float startx = x * PixelWidth;
                float starty = y * PixelHeight;
                float perpixelratiox = 1.0f / (float)Atlas.width;
                float perpixelratioy = 1.0f / (float)Atlas.height;
                startx *= perpixelratiox;
                starty *= perpixelratioy;
                float endx = startx + (perpixelratiox * PixelWidth);
                float endy = starty + (perpixelratioy * PixelHeight);
                UVMap m = new UVMap(_Images[count], new Vector2[]
                {
                    new Vector2(startx, starty),
                    new Vector2(startx, endy),
                    new Vector2(endx, starty),
                    new Vector2(endx, endy)
                });
                m.Register();
                count++;
            }
        }
    end:;
        _ATLAS = Atlas;
        File.WriteAllBytes("Assets/Resources/Textures/Atlas/atlas.png", Atlas.EncodeToPNG());
    }
}
