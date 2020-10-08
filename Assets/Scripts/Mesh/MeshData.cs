using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Class containing mesh data.
/// </summary>
public class MeshData
{
    /// <summary>
    /// List of vertices in this mesh.
    /// </summary>
    private readonly List<Vector3> vertices = new List<Vector3>();
    /// <summary>
    /// List of triangle indexes for this mesh.
    /// </summary>
    private readonly List<int> triangles = new List<int>();
    /// <summary>
    /// List of uv coordinates for the vertices of this mesh.
    /// </summary>
    private readonly List<Vector2> uvs = new List<Vector2>();


    /// <summary>
    /// Default Constructor: Creates an empty mesh data object.
    /// </summary>
    public MeshData()
    {

    }

    /// <summary>
    /// Specific Constructor: Creates a mesh data object with the given vertices, triangles and uv coordinates.
    /// </summary>
    /// <param name="vertices">The vertex list to initialize.</param>
    /// <param name="triangles">The triangle index list to initialize.</param>
    /// <param name="uvs">The uv coordinate array to initialize.</param>
    public MeshData(List<Vector3> vertices, List<int> triangles, Vector2[] uvs)
    {
        this.vertices.AddRange(vertices);
        this.triangles.AddRange(triangles);
        this.uvs.AddRange(uvs);
    }


    /// <summary>
    /// Offsets all of the vertex positions in mesh data by given amount.
    /// </summary>
    /// <param name="offset">The amount to offset vertex positions.</param>
    public void OffsetPosition(Vector3 offset)
    {
        for(int i = 0; i < this.vertices.Count; i++)
        {
            this.vertices[i] = this.vertices[i] + offset;
        }
    }

    /// <summary>
    /// Merges current mesh data with given mesh data.
    /// </summary>
    /// <param name="meshData">The mesh data to add to this mesh data.</param>
    public void Merge(MeshData meshData)
    {
        if(meshData == null || meshData.vertices.Count == 0)
        {
            return;
        }
        if(this.vertices.Count == 0)
        {
            this.vertices.AddRange(meshData.vertices);
            this.triangles.AddRange(meshData.triangles);
            this.uvs.AddRange(meshData.uvs);
            return;
        }
        int count = this.vertices.Count;
        this.vertices.AddRange(meshData.vertices);
        List<int> newtris = new List<int>();
        for(int i = 0; i < meshData.triangles.Count; i++)
        {
            newtris.Add(meshData.triangles[i] + count);
        }
        this.triangles.AddRange(newtris);
        this.uvs.AddRange(meshData.uvs);
    }

    /// <summary>
    /// Creates a Unity Mesh from mesh data.
    /// </summary>
    /// <returns>Returns the mesh created from this mesh data.</returns>
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this.vertices.ToArray(),
            triangles = this.triangles.ToArray(),
            uv = this.uvs.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        return mesh;
    }

    /// <summary>
    /// Clears all mesh data.
    /// </summary>
    public void Clear()
    {
        this.vertices.Clear();
        this.triangles.Clear();
        this.uvs.Clear();
    }
}
