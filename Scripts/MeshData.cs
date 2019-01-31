using System.Collections.Generic;

using UnityEngine;

// Class for creating MeshData
public class MeshData
{
    // MeshData objects
    private List<Vector3> _verts = new List<Vector3>();
    private List<int> _tris = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();

    // MeshData constructor
    public MeshData(List<Vector3> v, List<int> i, Vector2[] u)
    {
        this._verts = v;
        this._tris = i;
        this._uvs = new List<Vector2>(u);
    }

    // MeshData constructor
    public MeshData()
    {
        
    }

    // Add Position
    public void AddPos(Vector3 pos)
    {
        for(int i = 0; i < this._verts.Count; i++)
        {
            this._verts[i] = this._verts[i] + pos;
        }
    }

    // Merge Mesh Data
    public void Merge(MeshData data)
    {
        if(data._verts.Count <= 0)
        {
            return;
        }
        if(this._verts.Count <= 0)
        {
            this._verts = data._verts;
            this._tris = data._tris;
            this._uvs = data._uvs;
            return;
        }
        int count = this._verts.Count;
        this._verts.AddRange(data._verts);
        for(int i = 0; i < data._tris.Count; i++)
        {
            this._tris.Add(data._tris[i] + count);
        }
        this._uvs.AddRange(data._uvs);
    }

    // Sends verts, tris, and uvs to Mesh
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this._verts.ToArray(),
            triangles = this._tris.ToArray(),
            uv = this._uvs.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
