using SharpNoise.Modules;

using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    private static readonly int imagesize = 256;


    private static int STATICy = 0;
    private static float STATICyMultiplier = 0.01f;

    private static float STATICCutoff = 1.0f;

    private static float STATICPerlinFrequency = 0.04f;
    private static float STATICPerlinLacunarity = 2.34f;
    private static int STATICPerlinOctaveCount = 4;
    private static float STATICPerlinPersistance = 0.55f;
    private static int STATICPerlinSeed = 0;

    private static float STATICRidgedFrequency = 0.04f;
    private static float STATICRidgedLacunarity = 2.34f;
    private static int STATICRidgedOctaveCount = 4;
    private static int STATICRidgedSeed = 0;

    public int y = 0;
    public float yMultiplier = 0.02f;

    public float Cutoff = 1.0f;

    public float PerlinFrequency = 0.04f;
    public float PerlinLacunarity = 2.34f;
    public int PerlinOctaveCount = 4;
    public float PerlinPersistance = 0.55f;
    public int PerlinSeed = 0;

    public float RidgedFrequency = 0.04f;
    public float RidgedLacunarity = 2.34f;
    public int RidgedOctaveCount = 4;
    public int RidgedSeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = this.GetComponent<GameObject>();
        this.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
        this.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 1);
    }

    // Update is called once per frame
    void Update()
    {
        STATICy = this.y;
        STATICyMultiplier = this.yMultiplier;
        STATICCutoff = this.Cutoff;
        //STATICPerlinFrequency = this.PerlinFrequency;
        //STATICPerlinLacunarity = this.PerlinLacunarity;
        //STATICPerlinOctaveCount = this.PerlinOctaveCount;
        //STATICPerlinPersistance = this.PerlinPersistance;
        //STATICPerlinSeed = this.PerlinSeed;
        STATICRidgedFrequency = this.RidgedFrequency;
        STATICRidgedLacunarity = this.RidgedLacunarity;
        STATICRidgedOctaveCount = this.RidgedOctaveCount;
        STATICRidgedSeed = this.RidgedSeed;

        float[,] imageArray = new float[imagesize, imagesize];
        Texture2D texture = new Texture2D(imagesize, imagesize);
        for(int x = 0; x < imagesize; x++)
        {
            for(int z = 0; z < imagesize; z++)
            {
                imageArray[x, z] = this.GetNoise(x, STATICy, z);
                if(imageArray[x, z] > STATICCutoff)
                {
                    imageArray[x, z] = 1;
                }
                else
                {
                    imageArray[x, z] = 0;
                }

                if(imageArray[x, z] == 1)
                {
                    texture.SetPixel(x, z, Color.white);
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
    public float GetNoise(float px, float py, float pz)
    {
        //Perlin perlin = new Perlin()
        //{
        //    Frequency = STATICPerlinFrequency,
        //    Lacunarity = STATICPerlinLacunarity,
        //    OctaveCount = STATICPerlinOctaveCount,
        //    Persistence = STATICPerlinPersistance,
        //    Seed = STATICPerlinSeed,
        //};

        RidgedMulti ridged = new RidgedMulti()
        {
            Frequency = RidgedFrequency,
            Lacunarity = RidgedLacunarity,
            OctaveCount = RidgedOctaveCount,
            Seed = RidgedSeed,
        };

        return (float)ridged.GetValue(px, py, pz);
    }
}
