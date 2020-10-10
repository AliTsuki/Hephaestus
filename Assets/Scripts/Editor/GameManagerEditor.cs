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
		// Player Settings
		EditorGUILayout.LabelField("Player Settings", BoldCenteredStyle);
		this.gm.PlayerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab:", this.gm.PlayerPrefab, typeof(GameObject), false);
		this.gm.PlayerParent = (Transform)EditorGUILayout.ObjectField("Player Parent:", this.gm.PlayerParent, typeof(Transform), true);
		this.gm.BlockSelectorPrefab = (GameObject)EditorGUILayout.ObjectField("Block Selector Prefab:", this.gm.BlockSelectorPrefab, typeof(GameObject), false);

		// Chunk Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Chunk Settings", BoldCenteredStyle);
		this.gm.LevelGeometryLayerMask = EditorGUILayout.LayerField("Level Geometry Layer Mask:", this.gm.LevelGeometryLayerMask);
		this.gm.ChunkMaterial = (Material)EditorGUILayout.ObjectField("Chunk Material:", this.gm.ChunkMaterial, typeof(Material), false);
		this.gm.ChunkSize = EditorGUILayout.IntField("Chunk Size:", this.gm.ChunkSize);
		this.gm.StartingChunkRadius = EditorGUILayout.IntField("Starting Chunk Radius:", this.gm.StartingChunkRadius);
		this.gm.ActiveChunkRadius = EditorGUILayout.IntField("Active Chunk Radius:", this.gm.ActiveChunkRadius);
		this.gm.CutoffValue = EditorGUILayout.FloatField("Terrain Cutoff:", this.gm.CutoffValue);

		// Noise Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Noise Settings", BoldCenteredStyle);
		this.gm.Seed = EditorGUILayout.IntField("Seed:", this.gm.Seed);
		this.gm.NoiseType = (FastNoiseLite.NoiseType)EditorGUILayout.EnumPopup("Noise Type:", this.gm.NoiseType);
		this.gm.FractalType = (FastNoiseLite.FractalType)EditorGUILayout.EnumPopup("Fractal Type:", this.gm.FractalType);
		this.gm.Frequency = EditorGUILayout.FloatField("Frequency:", this.gm.Frequency);
		this.gm.Octaves = EditorGUILayout.IntField("Octaves:", this.gm.Octaves);
		this.gm.Lacunarity = EditorGUILayout.FloatField("Lacunarity:", this.gm.Lacunarity);
		this.gm.Persistence = EditorGUILayout.FloatField("Persistence:", this.gm.Persistence);
		this.gm.YMultiplier = EditorGUILayout.FloatField("Y Multiplier:", this.gm.YMultiplier);

		// Regenerate Button
		EditorGUILayout.Space();
		if(GUILayout.Button("Regenerate Starting Chunks") == true)
		{
			GameManager.Instance.RegenerateStartingChunks();
		}
		if(GUI.changed && EditorApplication.isPlaying == false)
		{
			EditorUtility.SetDirty(this.gm);
			EditorSceneManager.MarkSceneDirty(this.gm.gameObject.scene);
		}
	}
}
