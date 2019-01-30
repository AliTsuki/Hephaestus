﻿using System.Collections.Generic;

using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    public static int imagesize = 256;

    public float treedx = 5f;
    public float treedz = 5f;
    public float treemul = 0.6f;
    public float treeoffset = -1000f;
    public float dcutofftreemax = 0.166f;
    public float dcutofftreemin = 0.165f;
    public float treendx = 100f;
    public float treendz = 100f;
    public float treenmul = 0.4f;

    public static float Streedx = 5f;
    public static float Streedz = 5f;
    public static float Streemul = 0.6f;
    public static float Streeoffset = -1000f;
    public static float Sdcutofftreemax = 0.166f;
    public static float Sdcutofftreemin = 0.65f;
    public static float Streendx = 100f;
    public static float Streendz = 100f;
    public static float Streenmul = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
        this.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 1);
    }

    // Update is called once per frame
    void Update()
    {
        Streedx = this.treedx;
        Streedz = this.treedz;
        Streemul = this.treemul;
        Streeoffset = this.treeoffset;
        Sdcutofftreemax = this.dcutofftreemax;
        Sdcutofftreemin = this.dcutofftreemin;
        Streendx = this.treendx;
        Streendz = this.treendz;
        Streenmul = this.treenmul;
        float[,] imageArray = new float[imagesize, imagesize];
        Texture2D texture = new Texture2D(imagesize, imagesize);
        for(int x = 0; x < imagesize; x++)
        {
            for(int z = 0; z < imagesize; z++)
            {
                imageArray[x, z] = this.GetNoiseForTree(x, z);
                if(imageArray[x, z] > Sdcutofftreemin && imageArray[x, z] < Sdcutofftreemax)
                {
                    imageArray[x, z] = 1;
                }
                else
                {
                    imageArray[x, z] = 0;
                }

                if(imageArray[x, z] == 1)
                {
                    texture.SetPixel(x, z, Color.green);
                }
                else
                {
                    texture.SetPixel(x, z, Color.black);
                }
            }
        }
        texture.Apply();
        this.GetComponent<Renderer>().material.mainTexture = texture;
    }

    // Get noise tree generation
    public float GetNoiseForTree(float px, float pz)
    {
        float xz = Mathf.PerlinNoise(px / Streedx, pz / Streedz) * Streemul;
        float oxz = Mathf.PerlinNoise((px / Streendx) + Streeoffset, (pz / Streendz) - Streeoffset) * Streenmul;
        xz = (xz + oxz) / 2f;
        return xz;
    }
}