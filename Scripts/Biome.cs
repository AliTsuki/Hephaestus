using SharpNoise.Modules;

public class Biome
{
    // Biome List
    public static Biome Mountains = new Biome(1.4f, 0.9f, 0.6f, 0.04f, 0.015f, 2f, 2, 0.25f);
    public static Biome Forest =    new Biome(1.4f, 0.9f, 0.6f, 0.04f, 0.015f, 2f, 2, 0.25f);
    public static Biome Desert =    new Biome(1.4f, 0.9f, 0.6f, 0.04f, 0.015f, 2f, 2, 0.25f);
    // End of Biome List

    // Biome Map fields
    private static readonly float bmTempFreq = 1f;
    private static readonly float bmTempLac = 2f;
    private static readonly int bmTempOct = 2;
    private static readonly float bmTempPers = 0.25f;
    private static readonly float bmHumidFreq = 1f;
    private static readonly float bmHumidLac = 2f;
    private static readonly int bmHumidOct = 2;
    private static readonly float bmHumidPers = 0.25f;
    private static readonly int bmHumidOffset = 1000;
    // Noise fields
    public float YMultiplier;
    public float Land2NDLayerCutoff;
    public float LandTopLayerCutoff;
    public float AirAndLandIntersectionCutoff;
    public float PerlinFrequency;
    public float PerlinLacunarity;
    public int PerlinOctaveCount;
    public float PerlinPersistence;
    // Cave Noise fields
    public float CaveYMultiplier = 0.3f;
    public float CaveCutoff = 0.6f;
    public float RidgedFrequency = 0.03f;
    public float RidgedLacunarity = 2f;
    public int RidgedOctaveCount = 4;
    // Noise Generators
    private static readonly Perlin biomeMapTemp = new Perlin()
    {
        Frequency = bmTempFreq,
        Lacunarity = bmTempLac,
        OctaveCount = bmTempOct,
        Persistence = bmTempPers,
        Seed = GameManager.WorldSeed,
    };
    private static readonly Perlin biomeMapHumid = new Perlin()
    {
        Frequency = bmHumidFreq,
        Lacunarity = bmHumidLac,
        OctaveCount = bmHumidOct,
        Persistence = bmHumidPers,
        Seed = GameManager.WorldSeed + bmHumidOffset,
    };
    public Perlin perlin;
    public RidgedMulti ridged;

    public Biome(float airLandCutoff, float topLayerCutoff, float secLayerCutoff, float yMulti, float pFreq, float pLac, int pOct, float pPers)
    {
        this.AirAndLandIntersectionCutoff = airLandCutoff;
        this.LandTopLayerCutoff = topLayerCutoff;
        this.Land2NDLayerCutoff = secLayerCutoff;
        this.YMultiplier = yMulti;
        this.PerlinFrequency = pFreq;
        this.PerlinLacunarity = pLac;
        this.PerlinOctaveCount = pOct;
        this.PerlinPersistence = pPers;
        this.perlin = new Perlin()
        {
            Quality = SharpNoise.NoiseQuality.Fast,
            Frequency = this.PerlinFrequency,
            Lacunarity = this.PerlinLacunarity,
            OctaveCount = this.PerlinOctaveCount,
            Persistence = this.PerlinPersistence,
            Seed = GameManager.WorldSeed
        };
        this.ridged = new RidgedMulti()
        {
            Quality = SharpNoise.NoiseQuality.Fast,
            Frequency = this.RidgedFrequency,
            Lacunarity = this.RidgedLacunarity,
            OctaveCount = this.RidgedOctaveCount,
            Seed = GameManager.WorldSeed,
        };
    }

    public static Biome GetBiome(Int3 pos)
    {
        float temp = (float)biomeMapTemp.GetValue(pos.x, pos.y, pos.z);
        float humid = (float)biomeMapHumid.GetValue(pos.x, pos.y, pos.z);
        if(temp < 1 && temp > 0.5 && humid < 1 && humid > 0.5)
        {

        }
        return Mountains;
    }
}
