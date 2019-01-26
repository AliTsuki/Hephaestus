using UnityEngine;

// Class for Character Controller
public class Character : MonoBehaviour
{
    // Character objects
    public GameObject BlockSelector;
    public float MaxInteractableDistance = 8f;

    // Start is called before the first frame update
    // Character Start
    void Start()
    {

    }

    // Update is called once per frame
    // Character Update: Check Mouse clicks and Add or Delete Blocks
    // TODO: Limit placing blocks to not be inside player getting stuck, Add code for accepting tool and blocktype for adding/deleting
    void Update()
    {
        // Block Selector Cube
        Ray BlockSelectorRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if(Physics.Raycast(BlockSelectorRay, out RaycastHit BlockSelectorRayHit, this.MaxInteractableDistance))
        {
            this.BlockSelector.SetActive(true);
            this.BlockSelector.transform.GetComponent<MeshRenderer>().enabled = true;
            Vector3 rawposition = BlockSelectorRayHit.point - (BlockSelectorRayHit.normal * 0.5f);
            Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
            this.BlockSelector.transform.position = roundedposition;
        }
        else
        {
            if(this.BlockSelector.transform.GetComponent<MeshRenderer>().enabled == true && this.BlockSelector.activeSelf == true)
            {
                this.BlockSelector.SetActive(false);
                this.BlockSelector.transform.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        // On Right Click Down: Place Block
        if(Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.Raycast(ray, out RaycastHit hit, this.MaxInteractableDistance))
            {
                Vector3 rawposition = hit.point + (hit.normal * 0.5f);
                Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
                MathHelper.AddBlock(roundedposition, Block.Grass);
            }
        }

        // On Left Click Down Remove Block (set to Air)
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.Raycast(ray, out RaycastHit hit, this.MaxInteractableDistance))
            {
                Vector3 rawposition = hit.point - (hit.normal * 0.5f);
                Vector3 roundedposition = new Vector3(Mathf.RoundToInt(rawposition.x), Mathf.RoundToInt(rawposition.y), Mathf.RoundToInt(rawposition.z));
                MathHelper.AddBlock(roundedposition, Block.Air);
            }
        }
    }
}
