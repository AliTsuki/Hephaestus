using Unity.Entities;

using UnityEngine;

public struct MeshComponent : IComponentData
{
    public Mesh Mesh;
    public MeshCollider MeshCollider;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;
}
