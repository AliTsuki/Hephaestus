using Assets.ModLib;

using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkIntegrator : MonoBehaviour
{
    public Chunk chunk;

    // Start is called before the first frame update
    void Start()
    {
        Chunk chunk = new Chunk(this.gameObject);
        var texture = SimpleModLoader.GetModLoader().GetBlockTextureLoader().GetTexture();
        this.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Tex2DArray", texture);
        chunk.RedrawChunk();
        var mesh = chunk.GetMesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
