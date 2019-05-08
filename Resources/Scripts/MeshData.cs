using System.Collections.Generic;

using UnityEngine;

// Class for creating MeshData
public class MeshData
{
    // MeshData objects
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    // MeshData constructor with parameters
    public MeshData(List<Vector3> v, List<int> i, Vector2[] u)
    {
        this.verts.AddRange(v);
        this.tris.AddRange(i);
        this.uvs.AddRange(u);
    }

    // MeshData default constructor
    public MeshData()
    {
        
    }

    // Add Position
    public void AddPos(Vector3 pos)
    {
        for(int i = 0; i < this.verts.Count; i++)
        {
            this.verts[i] = this.verts[i] + pos;
        }
    }

    // Merge Mesh Data
    public void Merge(MeshData data)
    {
        if(data.verts.Count <= 0)
        {
            return;
        }
        if(this.verts.Count <= 0)
        {
            this.verts.AddRange(data.verts);
            this.tris.AddRange(data.tris);
            this.uvs.AddRange(data.uvs);
            return;
        }
        int count = this.verts.Count;
        this.verts.AddRange(data.verts);
        List<int> newtris = new List<int>();
        for(int i = 0; i < data.tris.Count; i++)
        {
            newtris.Add(data.tris[i] + count);
        }
        this.tris.AddRange(newtris);
        this.uvs.AddRange(data.uvs);
    }

    // Sends verts, tris, and uvs to Mesh
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this.verts.ToArray(),
            triangles = this.tris.ToArray(),
            uv = this.uvs.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
