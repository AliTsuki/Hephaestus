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

    /// <summary>
    /// The current active player.
    /// </summary>
    public Player Player;
    /// <summary>
    /// The prefab to instantiate for the player character.
    /// </summary>
    public GameObject PlayerPrefab;
    /// <summary>
    /// The parent transform to put the player under.
    /// </summary>
    public Transform PlayerParent;
    /// <summary>
    /// The prefab for the block selector box.
    /// </summary>
    public GameObject BlockSelectorPrefab;
    /// <summary>
    /// The maximum distance that a block can be interacted with.
    /// </summary>
    public float BlockSelectionMaxDistance = 5f;

    /// <summary>
    /// The layer that the game world geometry is on.
    /// </summary>
    public LayerMask LevelGeometryLayerMask;

    /// <summary>
    /// The prefab for the chunk objects.
    /// </summary>
    public GameObject ChunkPrefab;
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
    /// The number of chunks to generate per column.
    /// </summary>
    public int ChunksPerColumn = 16;
    /// <summary>
    /// The amount of chunks generated for the starting area is this value doubled minus 1 then cubed.
    /// </summary>
    public int StartingColumnRadius = 9;
    /// <summary>
    /// The radius of chunks to generate continously as the player explores the world.
    /// </summary>
    public int ActiveColumnRadius = 7;

    /// <summary>
    /// The seed to use for all noise generation.
    /// </summary>
    public int Seed = 0;

    /// <summary>
    /// The noise generator for the terrain.
    /// </summary>
    public FastNoiseLite NoiseGenerator = new FastNoiseLite();
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
    public float Frequency = 0.0015f;
    /// <summary>
    /// The number of octaves to use if using fractalized noise generation.
    /// </summary>
    public int Octaves = 4;
    /// <summary>
    /// The lacunarity of the noise generation.
    /// </summary>
    public float Lacunarity = 3f;
    /// <summary>
    /// The persistence of the noise generation.
    /// </summary>
    public float Persistence = 0.8f;

    /// <summary>
    /// The noise generator for the terrain.
    /// </summary>
    public FastNoiseLite NoiseGenerator2 = new FastNoiseLite();
    /// <summary>
    /// The noise type to use for the noise generation.
    /// </summary>
    public FastNoiseLite.NoiseType NoiseType2 = FastNoiseLite.NoiseType.Perlin;
    /// <summary>
    /// the fractal type to use for the noise generation.
    /// </summary>
    public FastNoiseLite.FractalType FractalType2 = FastNoiseLite.FractalType.FBm;
    /// <summary>
    /// The frequency of the noise generation.
    /// </summary>
    public float Frequency2 = 0.0015f;
    /// <summary>
    /// The number of octaves to use if using fractalized noise generation.
    /// </summary>
    public int Octaves2 = 4;
    /// <summary>
    /// The lacunarity of the noise generation.
    /// </summary>
    public float Lacunarity2 = 3f;
    /// <summary>
    /// The persistence of the noise generation.
    /// </summary>
    public float Persistence2 = 0.8f;

    public enum NoiseCombinationEnum
    {
        Min,
        Max,
        Average,
        Just1,
        Just2
    }
    /// <summary>
    /// How to combine noise and noise2.
    /// </summary>
    public NoiseCombinationEnum NoiseCombination = GameManager.NoiseCombinationEnum.Average;
    /// <summary>
    /// The multiplier to add to the noise generation values.
    /// </summary>
    public float YMultiplier = 0.008f;
    /// <summary>
    /// The cutoff value to differentiate air from land.
    /// </summary>
    public float CutoffValue = 0.5f;

    /// <summary>
    /// The minimum nuber of objects to keep in the object pooler.
    /// </summary>
    public int MinPooledObjects = 100;

    /// <summary>
    /// The default player character height.
    /// </summary>
    public const float DefaultCharacterHeight = 1.8f;
    /// <summary>
    /// The player character height while crouching.
    /// </summary>
    public const float CrouchCharacterHeight = 0.9f;
    /// <summary>
    /// The rate to lerp from default height to crouching height for the player.
    /// </summary>
    public const float CrouchRate = 0.1f;

    /// <summary>
    /// The field of view of the camera while sprinting.
    /// </summary>
    public const float SprintFOV = 70f;
    /// <summary>
    /// The default field of view for the camera.
    /// </summary>
    public const float DefaultFOV = 60f;

    /// <summary>
    /// The file path where all opaque block textures are kept.
    /// </summary>
    public const string OpaqueBlockTexturePath = "Assets/Textures/Blocks/Opaque/";
    /// <summary>
    /// The path to save the texture atlas to.
    /// </summary>
    public const string TextureAtlasPath = "Assets/Textures/Blocks/Atlas/atlas.png";


    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        Instance = this;
        Logger.Start();
        this.ChunkParentTransform = new GameObject("Chunks").transform;
        this.UpdateNoiseGenerators();
        TextureAtlas.CreateAtlas();
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

        this.NoiseGenerator2.SetSeed(this.Seed);
        this.NoiseGenerator2.SetNoiseType(this.NoiseType2);
        this.NoiseGenerator2.SetFractalType(this.FractalType2);
        this.NoiseGenerator2.SetFrequency(this.Frequency2);
        this.NoiseGenerator2.SetFractalOctaves(this.Octaves2);
        this.NoiseGenerator2.SetFractalLacunarity(this.Lacunarity2);
        this.NoiseGenerator2.SetFractalGain(this.Persistence2);
    }
}
