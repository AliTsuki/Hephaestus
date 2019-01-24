using UnityEngine;

public class World : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for(int x = 0; x < 12; x++)
            for(int z = 0; z < 12; z++)
            {
                var o = Transform.Instantiate<Transform>(Resources.Load<Transform>("Prefabs/Chunk")) as Transform;
                o.transform.position = new Vector3(x * 12, 0, z * 12);
                o.name = $@"C_{x}_{z}";
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
