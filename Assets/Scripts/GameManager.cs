using UnityEngine;


/// <summary>
/// Singleton class that exists as a GameObject in scene that sends out messages to all the game's systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static GameManager Instance { get; private set; }

    public GameObject PlayerPrefab;
    public Transform PlayerParent;
    public Player Player;
    public LayerMask LevelGeometryLayerMask;

    // Height
    public const float DefaultCharacterHeight = 1.8f;
    public const float CrouchCharacterHeight = 0.9f;
    public const float CrouchRate = 0.1f;

    // FOV
    public const float SprintFOV = 70f;
    public const float DefaultFOV = 60f;
    public const float ADSFOV = 40f;

    /// <summary>
    /// The transform to act as parent for all the chunks.
    /// </summary>
    public Transform ChunkParentTransform;
    /// <summary>
    /// The material used to render all the chunks.
    /// </summary>
    public Material ChunkMaterial;
    /// <summary>
    /// The size in blocks of each chunk. Chunks are this many blocks cubed.
    /// </summary>
    public int ChunkSize = 16;
    /// <summary>
    /// The amount of chunks generated for the starting area is this value doubled minus 1 then cubed.
    /// </summary>
    public int StartingChunkRadius = 3;

    /// <summary>
    /// The noise generator for the terrain.
    /// </summary>
    public FastNoiseLite NoiseGenerator = new FastNoiseLite();
    /// <summary>
    /// The seed to use for all noise generation.
    /// </summary>
    public int Seed = 0;
    /// <summary>
    /// The noise type to use for the noise generation.
    /// </summary>
    public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.Perlin;
    /// <summary>
    /// the fractal type to use for the noise generation.
    /// </summary>
    public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.FBm;
    /// <summary>
    /// The frequency of the noise generation.
    /// </summary>
    public float Frequency = 0.009f;
    /// <summary>
    /// The number of octaves to use if using fractalized noise generation.
    /// </summary>
    public int Octaves = 4;
    /// <summary>
    /// The lacunarity of the noise generation.
    /// </summary>
    public float Lacunarity = 4f;
    /// <summary>
    /// The persistence of the noise generation.
    /// </summary>
    public float Persistence = 0.2f;
    /// <summary>
    /// The multiplier to add to the noise generation values.
    /// </summary>
    public float YMultiplier = 1f;
    /// <summary>
    /// The cutoff value to differentiate air from land.
    /// </summary>
    public float CutoffValue = 0.5f;


    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        Instance = this;
        Logger.Start();
        this.ChunkParentTransform = new GameObject("Chunks").transform;
        this.UpdateNoiseGenerators();
    }

    // Start is called before the first frame update.
    private void Start()
    {
        World.WorldThreadStart();
    }

    // Update is called once per frame.
    private void Update()
    {
        World.MainThreadUpdate();
    }

    // Fixed update is called a fixed number of times per second.
    private void FixedUpdate()
    {
        
    }

    // OnApplicationQuit is called before the application is quit.
    private void OnApplicationQuit()
    {
        World.Quit();
        Logger.Quit();
    }

    /// <summary>
    /// Tells the World object to regenerate all current chunks.
    /// </summary>
    public void RegenerateStartingChunks()
    {
        if(World.HasGenerated == true)
        {
            this.UpdateNoiseGenerators();
            World.RestartWorld();
        }
    }

    /// <summary>
    /// Updates the noise generators by passing them the current noise generator values set in the inspector window.
    /// </summary>
    private void UpdateNoiseGenerators()
    {
        this.NoiseGenerator.SetSeed(this.Seed);
        this.NoiseGenerator.SetNoiseType(this.NoiseType);
        this.NoiseGenerator.SetFractalType(this.FractalType);
        this.NoiseGenerator.SetFrequency(this.Frequency);
        this.NoiseGenerator.SetFractalOctaves(this.Octaves);
        this.NoiseGenerator.SetFractalLacunarity(this.Lacunarity);
        this.NoiseGenerator.SetFractalGain(this.Persistence);
    }
}
