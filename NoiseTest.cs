using System.Collections.Generic;

using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    float Div = 18;
    float mul = 1.4f;
    float cutoff = 0.69f;

    // Update is called once per frame
    void Update()
    {
        float[,] img = new float[384, 384];
        List<float> c = new List<float>();
        Texture2D texture = new Texture2D(384, 384);
        for(int x = 0; x < 384; x++)
            for(int y = 0; y < 384; y++)
            {
                img[x, y] = GetNoise(x, y);
                c.Add(img[x, y]);
                if(img[x, y] > cutoff)
                    img[x, y] = 1;
                else
                    img[x, y] = 0;
                texture.SetPixel(x, y, img[x, y] == 1 ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 0));
            }
        texture.Apply();
        this.GetComponent<Renderer>().material.mainTexture = texture;
    }

    float GetNoise(float x, float y)
    {
        var perlin = Mathf.PerlinNoise(x / Div, y / Div) * mul;
        return perlin;
    }
}
