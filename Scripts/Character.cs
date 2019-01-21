using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for Character Controller
public class Character : MonoBehaviour
{
    // Character objects
    public Transform Add;
    public Transform Delete;

    // Start is called before the first frame update
    // Character Start: Instantiate Add and Delete GameObjects as Transforms
    void Start()
    {
        this.Add = Transform.Instantiate(Resources.Load<Transform>("Prefabs/Add")) as Transform;
        this.Delete = Transform.Instantiate(Resources.Load<Transform>("Prefabs/Delete")) as Transform;
    }

    // Update is called once per frame
    // Character Update: Check Mouse clicks and Add or Delete Blocks
    // TODO: Add code for accepting tool and blocktype for adding/deleting, change ADD/DELETE object to be a cube highlighting block pointed at
    void Update()
    {
        //Place Block : Right Click
        if(Input.GetMouseButtonDown(1))
        {
            this.Add.transform.GetComponent<MeshRenderer>().enabled = true;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 rawposition = hit.point + hit.normal;
                Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
                Debug.Log("Placing block: hit.point = " + hit.point.ToString());
                Debug.Log("Placing block: hit.normal = " + hit.normal.ToString());
                Debug.Log("Placing block: roundedposition = " + roundedposition.ToString());
                this.Add.transform.position = roundedposition;
                MathHelper.AddBlock(roundedposition, Block.Stone);
            }
        }
        else
        {
            if(this.Add.transform.GetComponent<MeshRenderer>().enabled == true)
            {

            }
            this.Add.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        // Remove Block : Left Click
        if(Input.GetMouseButtonDown(0))
        {
            this.Delete.transform.GetComponent<MeshRenderer>().enabled = true;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 rawposition = hit.point - hit.normal;
                Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
                Debug.Log("Removing block: hit.point = " + hit.point.ToString());
                Debug.Log("Removing block: hit.normal = " + hit.normal.ToString());
                Debug.Log("Removing block: roundedposition = " + roundedposition.ToString());
                this.Delete.transform.position = roundedposition;
                MathHelper.AddBlock(roundedposition, Block.Air);
            }
        }
        else
        {
            if(this.Delete.transform.GetComponent<MeshRenderer>().enabled == true)
            {

            }
            this.Delete.transform.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
