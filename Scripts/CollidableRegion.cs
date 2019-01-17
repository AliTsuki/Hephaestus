using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidableRegion
{
    Vector3Int size;
    GameObject parent;
    BoxCollider[,,] colliders;

    public CollidableRegion(int x, int y, int z)
    {
        size = new Vector3Int(x, y, z);
    }

    public void CreateGameObjectPool()
    {
        parent = new GameObject("ColliadableRegion");
        colliders = new BoxCollider[size.x, size.y, size.z];

        Vector3 position;

        for(int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    GameObject go = new GameObject();
                    go.transform.SetParent(parent.transform);

                    position.x = (i - size.x / 2f) + 0.5f;
                    position.y = (j - size.y / 2f) + 0.5f;
                    position.z = (k - size.z / 2f) + 0.5f;

                    colliders[i, j, k] = go.AddComponent<BoxCollider>();
                    colliders[i, j, k].enabled = false;
                }
            }
        }
    }

    public IEnumerator SetCollidersAt(Vector3 center)
    {
        yield return null;
    }

    public void ShiftColliders(Direction direction, Vector3 position)
    {

    }

    int DimensionToIndex(int x, int y, int z)
    {
        return x * size.y * size.z + y * size.z + z;
    }
}
