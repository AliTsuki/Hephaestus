using System;

using SharpNoise;

using Unity.Mathematics;

public static class Noise
{

    public static float GetNoise(int3 pos, float frequency, float lacunarity, int octaveCount, float persistence, int seed, int height, float heightMultiplier, float yMultiplier)
    {
        return GetValuePerlin(pos, frequency, lacunarity, octaveCount, persistence, seed) + ((pos.y) - (height * heightMultiplier) * yMultiplier);
        //return (float)biome.Perlin.GetValue(pos.x, pos.y, pos.z) + ((pos.y - (128 * 0.3f)) * biome.YMultiplier);
    }

    public static float GetNoiseCave(int3 pos, float frequencyCave, float lacunarityCave, int octaveCountCave, float persistenceCave, int seed, int heightCave, float heightMultiplierCave, float yMultiplierCave)
    {
        return GetValueRidged(pos, frequencyCave, lacunarityCave, octaveCountCave, persistenceCave, seed) + ((pos.y) - (heightCave / heightMultiplierCave) * yMultiplierCave);
        //return (float)biome.Ridged.GetValue(pos.x, pos.y, pos.z) - (pos.y / (128 * 0.5f) * biome.CaveYMultiplier);
    }

    private static float GetValuePerlin(float3 pos, float frequency, float lacunarity, int octaveCount, float persistence, int seed)
    {
        float value = 0f;
        float signal = 0f;
        float currentPersistence = 1f;
        int currentSeed;
        pos.x *= frequency;
        pos.y *= frequency;
        pos.z *= frequency;
        for(var currentOctave = 0; currentOctave < octaveCount; currentOctave++)
        {
            currentSeed = (seed + currentOctave) & int.MaxValue;
            signal = (float)NoiseGenerator.GradientCoherentNoise3D(pos.x, pos.y, pos.z, currentSeed);
            value += signal * currentPersistence;
            pos.x *= lacunarity;
            pos.y *= lacunarity;
            pos.z *= lacunarity;
            currentPersistence *= persistence;
        }
        return value;
    }

    public static float GetValueRidged(float3 pos, float frequency, float lacunarity, int octaveCount, float persistence, int seed)
    {
        pos.x *= frequency;
        pos.y *= frequency;
        pos.z *= frequency;
        float signal = 0.0f;
        float value = 0.0f;
        float weight = 1.0f;
        float offset = 1.0f;
        float gain = 2.0f;
        for(var curOctave = 0; curOctave < octaveCount; curOctave++)
        {
            int currentSeed = (seed + curOctave) & 0x7fffffff;
            signal = (float)NoiseGenerator.GradientCoherentNoise3D(pos.x, pos.y, pos.z, currentSeed);
            signal = offset - Math.Abs(signal);
            signal *= signal;
            signal *= weight;
            weight = signal * gain;
            if(weight > 1.0)
                weight = 1.0f;
            if(weight < 0.0)
                weight = 0.0f;
            value += (signal * GetSpectralWeights(curOctave, lacunarity));
            pos.x *= lacunarity;
            pos.y *= lacunarity;
            pos.z *= lacunarity;
        }
        return (value * 1.25f) - 1.0f;
    }

    private static float GetSpectralWeights(int currentOctave, float lacunarityCave)
    {
        int maxOctaves = 10;
        float h = 1.0f;
        float[] spectralWeights = new float[maxOctaves];
        float frequency = 1.0f;
        for(var i = 0; i < maxOctaves; i++)
        {
            spectralWeights[i] = (float)Math.Pow(frequency, -h);
            frequency *= lacunarityCave;
        }
        return spectralWeights[currentOctave];
    }
}
