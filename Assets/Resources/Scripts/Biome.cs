using SharpNoise.Modules;

namespace OLD
{
    public class Biome
    {
        // Biome List
        public static Biome Mountains = new Biome(1.4f, 0.9f, 0.6f, 0.04f, 0.015f, 2f, 2, 0.25f);
        public static Biome Forest = new Biome(1.4f, 0.9f, 0.6f, 0.04f, 0.015f, 2f, 2, 0.25f);
        public static Biome Desert = new Biome(1.4f, 0.9f, 0.6f, 0.04f, 0.015f, 2f, 2, 0.25f);
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
        public Perlin Perlin;
        public RidgedMulti Ridged;

        public Biome(float _airLandCutoff, float _topLayerCutoff, float _secLayerCutoff, float _yMulti, float _pFreq, float _pLac, int _pOct, float _pPers)
        {
            this.AirAndLandIntersectionCutoff = _airLandCutoff;
            this.LandTopLayerCutoff = _topLayerCutoff;
            this.Land2NDLayerCutoff = _secLayerCutoff;
            this.YMultiplier = _yMulti;
            this.PerlinFrequency = _pFreq;
            this.PerlinLacunarity = _pLac;
            this.PerlinOctaveCount = _pOct;
            this.PerlinPersistence = _pPers;
            this.Perlin = new Perlin()
            {
                Quality = SharpNoise.NoiseQuality.Fast,
                Frequency = this.PerlinFrequency,
                Lacunarity = this.PerlinLacunarity,
                OctaveCount = this.PerlinOctaveCount,
                Persistence = this.PerlinPersistence,
                Seed = GameManager.WorldSeed
            };
            this.Ridged = new RidgedMulti()
            {
                Quality = SharpNoise.NoiseQuality.Fast,
                Frequency = this.RidgedFrequency,
                Lacunarity = this.RidgedLacunarity,
                OctaveCount = this.RidgedOctaveCount,
                Seed = GameManager.WorldSeed,
            };
        }

        // TODO: add in BiomeMap stuffs
        public static Biome GetBiome(Int3 _pos)
        {
            float temp = (float)biomeMapTemp.GetValue(_pos.x, _pos.y, _pos.z);
            float humid = (float)biomeMapHumid.GetValue(_pos.x, _pos.y, _pos.z);
            if(temp < 1 && temp > 0.5 && humid < 1 && humid > 0.5)
            {

            }
            return Mountains;
        }
    }

}
