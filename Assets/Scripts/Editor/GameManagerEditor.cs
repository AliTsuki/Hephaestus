using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;


/// <summary>
/// Class describing how to render the custom inspector window for the GameManager script object.
/// </summary>
[CustomEditor(typeof(GameManager)), System.Serializable]
public class GameManagerEditor : Editor
{
    private GameManager gm;


    // OnEnable is called when the object is enabled and becomes active.
    public void OnEnable()
    {
		this.gm = (GameManager)this.target;
	}

    // OnInspectorGUI is called every time the custom inspector window is modified.
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
		GUIStyle BoldCenteredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, };
		this.gm.WorldSaveName = EditorGUILayout.TextField("World Save Name:", this.gm.WorldSaveName);
		// Player Settings
		EditorGUILayout.LabelField("Player Settings", BoldCenteredStyle);
		this.gm.PlayerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab:", this.gm.PlayerPrefab, typeof(GameObject), false);
		this.gm.PlayerParent = (Transform)EditorGUILayout.ObjectField("Player Parent:", this.gm.PlayerParent, typeof(Transform), true);
		this.gm.BlockSelectorPrefab = (GameObject)EditorGUILayout.ObjectField("Block Selector Prefab:", this.gm.BlockSelectorPrefab, typeof(GameObject), false);
		this.gm.BlockSelectionMaxDistance = EditorGUILayout.FloatField("Block Selection Max Distance:", this.gm.BlockSelectionMaxDistance);

		// Chunk Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Chunk Settings", BoldCenteredStyle);
		this.gm.LevelGeometryLayerMask = EditorGUILayout.LayerField("Level Geometry Layer Mask:", this.gm.LevelGeometryLayerMask);
		this.gm.ChunkPrefab = (GameObject)EditorGUILayout.ObjectField("Chunk Prefab:", this.gm.ChunkPrefab, typeof(GameObject), false);
		this.gm.ChunkMaterial = (Material)EditorGUILayout.ObjectField("Chunk Material:", this.gm.ChunkMaterial, typeof(Material), false);
		this.gm.ActiveColumnRadius = EditorGUILayout.IntField("Active Column Radius:", this.gm.ActiveColumnRadius);

		// Noise Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Noise Settings", BoldCenteredStyle);
		this.gm.Seed = EditorGUILayout.IntField("Seed:", this.gm.Seed);
		this.gm.NoiseCombination = (GameManager.NoiseCombinationEnum)EditorGUILayout.EnumPopup("Noise Combination:", this.gm.NoiseCombination);
		this.gm.YMultiplier = EditorGUILayout.FloatField("Y Multiplier:", this.gm.YMultiplier);
		this.gm.CutoffValue = EditorGUILayout.FloatField("Terrain Cutoff:", this.gm.CutoffValue);

		// Noise Generator Base Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Noise Generator Base", BoldCenteredStyle);
		this.gm.NoiseTypeBase = (FastNoiseLite.NoiseType)EditorGUILayout.EnumPopup("Noise Type Base:", this.gm.NoiseTypeBase);
		this.gm.FractalTypeBase = (FastNoiseLite.FractalType)EditorGUILayout.EnumPopup("Fractal Type Base:", this.gm.FractalTypeBase);
		this.gm.FrequencyBase = EditorGUILayout.FloatField("Frequency Base:", this.gm.FrequencyBase);
		this.gm.OctavesBase = EditorGUILayout.IntField("Octaves Base:", this.gm.OctavesBase);
		this.gm.LacunarityBase = EditorGUILayout.FloatField("Lacunarity Base:", this.gm.LacunarityBase);
		this.gm.PersistenceBase = EditorGUILayout.FloatField("Persistence Base:", this.gm.PersistenceBase);
		this.gm.InvertBase = EditorGUILayout.Toggle("Invert Base?:", this.gm.InvertBase);

		// Noise Generator Ridged Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Noise Generator Ridged", BoldCenteredStyle);
		this.gm.NoiseTypeRidged = (FastNoiseLite.NoiseType)EditorGUILayout.EnumPopup("Noise Type Ridged:", this.gm.NoiseTypeRidged);
		this.gm.FractalTypeRidged = (FastNoiseLite.FractalType)EditorGUILayout.EnumPopup("Fractal Type Ridged:", this.gm.FractalTypeRidged);
		this.gm.FrequencyRidged = EditorGUILayout.FloatField("Frequency Ridged:", this.gm.FrequencyRidged);
		this.gm.OctavesRidged = EditorGUILayout.IntField("Octaves Ridged:", this.gm.OctavesRidged);
		this.gm.LacunarityRidged = EditorGUILayout.FloatField("Lacunarity Ridged:", this.gm.LacunarityRidged);
		this.gm.PersistenceRidged = EditorGUILayout.FloatField("Persistence Ridged:", this.gm.PersistenceRidged);
		this.gm.InvertRidged = EditorGUILayout.Toggle("Invert Ridged?:", this.gm.InvertRidged);

		// Noise Generator Cave Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Noise Generator Caves", BoldCenteredStyle);
		this.gm.CaveWormPositionNoiseType = (FastNoiseLite.NoiseType)EditorGUILayout.EnumPopup("Cave Position Noise Type:", this.gm.CaveWormPositionNoiseType);
		this.gm.CaveWormPositionFrequency = EditorGUILayout.FloatField("Cave Position Frequency:", this.gm.CaveWormPositionFrequency);
		this.gm.CaveWormDirectionNoiseType = (FastNoiseLite.NoiseType)EditorGUILayout.EnumPopup("Cave Direction Noise Type:", this.gm.CaveWormDirectionNoiseType);
		this.gm.CaveWormDirectionFrequency = EditorGUILayout.FloatField("Cave Direction Frequency:", this.gm.CaveWormDirectionFrequency);
		this.gm.MinimumCaveWorms = EditorGUILayout.IntField("Min Caves:", this.gm.MinimumCaveWorms);
		this.gm.MaximumCaveWorms = EditorGUILayout.IntField("Max Caves:", this.gm.MaximumCaveWorms);
		this.gm.MaxWormChunkDistance = EditorGUILayout.IntField("Max Cave Chunk Distance:", this.gm.MaxWormChunkDistance);
		this.gm.MaxWormSegments = EditorGUILayout.IntField("Max Cave Segments:", this.gm.MaxWormSegments);
		this.gm.MinimumCaveWormRadius = EditorGUILayout.IntField("Min Cave Radius:", this.gm.MinimumCaveWormRadius);
		this.gm.MaximumCaveWormRadius = EditorGUILayout.IntField("Max Cave Radius:", this.gm.MaximumCaveWormRadius);

		if(GUI.changed && EditorApplication.isPlaying == false)
		{
			EditorUtility.SetDirty(this.gm);
			EditorSceneManager.MarkSceneDirty(this.gm.gameObject.scene);
		}
	}
}
