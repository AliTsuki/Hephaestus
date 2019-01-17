using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: add MoveCollider function, add EditCollider function
public class GameController : MonoBehaviour
{
    public static GameController instance;

    public int blocksTall = 256;

    public Texture texture;
    public string texturePath = "";

    public Material worldTextures;
    public GameObject playerPrefab;

    public ColliderController colliderController;

    World world;

    // Use this for initialization
    void Awake()
    {
        instance = this;

        world = new World(blocksTall, worldTextures);

        TextureController.Initialize(texturePath, texture);

        UnityEngine.Profiling.Profiler.BeginSample("Create GO Pool");

        colliderController = new ColliderController();
        colliderController.CreateGameObjectPool();

        UnityEngine.Profiling.Profiler.EndSample();

        Instantiate(playerPrefab, new Vector3(0, blocksTall, 0), Quaternion.identity);

        StartCoroutine(colliderController.SetCollidersAtChunk(0, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        world.Update();
    }
}
