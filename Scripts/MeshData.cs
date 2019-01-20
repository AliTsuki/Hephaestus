using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for creating MeshData
public class MeshData
{
    private List<Vector3> _Verts = new List<Vector3>();
    private List<int> _Tris = new List<int>();
    private List<Vector2> _UVs = new List<Vector2>();

    public MeshData(List<Vector3> v, List<int> i, Vector2[] u)
    {
        _Verts = v;
        _Tris = i;
        _UVs = new List<Vector2>(u);
    }

    public MeshData()
    {
        
    }

    public void AddPos(Vector3 loc)
    {
        for(int i = 0; i < _Verts.Count; i++)
        {
            _Verts[i] = _Verts[i] + loc;
        }
    }

    public void Merge(MeshData m)
    {
        if(m._Verts.Count <= 0)
        {
            return;
        }
        if(_Verts.Count <= 0)
        {
            _Verts = m._Verts;
            _Tris = m._Tris;
            _UVs = m._UVs;
            return;
        }
        int count = _Verts.Count;
        _Verts.AddRange(m._Verts);
        for (int i = 0; i < m._Tris.Count; i++)
        {
            _Tris.Add(m._Tris[i] + count);
        }
        _UVs.AddRange(m._UVs);
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = _Verts.ToArray();
        mesh.triangles = _Tris.ToArray();
        mesh.uv = _UVs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
