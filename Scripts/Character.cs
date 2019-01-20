using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for Character Controller
public class Character : MonoBehaviour
{
    public Transform Add;
    public Transform Delete;

    // Start is called before the first frame update
    void Start()
    {
        Add = Transform.Instantiate(Add) as Transform;
        Delete = Transform.Instantiate(Delete) as Transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Add.transform.GetComponent<MeshRenderer>().enabled = true;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Vector3 rawposition = hit.point + hit.normal;
                Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
                Add.transform.position = roundedposition;
                MathHelper.AddBlock(roundedposition, Block.Stone);
            }
        }
        else
        {
            if (Add.transform.GetComponent<MeshRenderer>().enabled == true)
            {

            }
            Add.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        if (Input.GetMouseButton(1))
        {
            Delete.transform.GetComponent<MeshRenderer>().enabled = true;

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 rawposition = hit.point - hit.normal;
                Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
                Delete.transform.position = roundedposition;
                MathHelper.AddBlock(roundedposition, Block.Air);
            }
        }
        else
        {
            if(Delete.transform.GetComponent<MeshRenderer>().enabled == true)
            {

            }
            Delete.transform.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
