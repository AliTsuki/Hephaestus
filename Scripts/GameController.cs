using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        Instantiate(playerPrefab, new Vector3(8, blocksTall, 8), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        world.Update();
    }
}
