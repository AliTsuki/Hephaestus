using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static Matrix4x4 id = Matrix4x4.identity;
    public Material material;

    Chunk chunk;

    // Start is called before the first frame update
    void Start()
    {
        chunk = new Chunk(new Vector3Int(0, 0, 0));
        chunk.GenerateBlockArray();

        StartCoroutine(chunk.GenerateMesh());
    }

    // Update is called once per frame
    void Update()
    {
        if (chunk.ready)
            Graphics.DrawMesh(chunk.mesh, id, material, 0);
    }
}
