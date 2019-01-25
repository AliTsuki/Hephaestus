using System.Collections.Generic;

using UnityEngine;

// Class for creating MeshData
public class MeshData
{
    // MeshData objects
    private List<Vector3> _Verts = new List<Vector3>();
    private List<int> _Tris = new List<int>();
    private List<Vector2> _UVs = new List<Vector2>();

    // MeshData constructor
    public MeshData(List<Vector3> v, List<int> i, Vector2[] u)
    {
        this._Verts = v;
        this._Tris = i;
        this._UVs = new List<Vector2>(u);
    }

    // MeshData constructor (EMPTY, can be deleted??)
    public MeshData()
    {
        
    }

    // Add Position
    public void AddPos(Vector3 loc)
    {
        for(int i = 0; i < this._Verts.Count; i++)
        {
            this._Verts[i] = this._Verts[i] + loc;
        }
    }

    // Merge Mesh Data
    public void Merge(MeshData m)
    {
        if(m._Verts.Count <= 0)
        {
            return;
        }
        if(this._Verts.Count <= 0)
        {
            this._Verts = m._Verts;
            this._Tris = m._Tris;
            this._UVs = m._UVs;
            return;
        }
        int count = this._Verts.Count;
        this._Verts.AddRange(m._Verts);
        for(int i = 0; i < m._Tris.Count; i++)
        {
            this._Tris.Add(m._Tris[i] + count);
        }
        this._UVs.AddRange(m._UVs);
    }

    // Sends verts, tris, and uvs to Mesh
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this._Verts.ToArray(),
            triangles = this._Tris.ToArray(),
            uv = this._UVs.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
