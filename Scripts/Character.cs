using UnityEngine;

// Class for Character Controller
public class Character : MonoBehaviour
{
    // Character fields
    public GameObject BlockSelector;
    public static float MaxInteractableDistance = 8f;

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
        if(Physics.Raycast(BlockSelectorRay, out RaycastHit BlockSelectorRayHit, MaxInteractableDistance))
        {
            this.BlockSelector.SetActive(true);
            this.BlockSelector.transform.GetComponent<MeshRenderer>().enabled = true;
            Vector3 rawPos = BlockSelectorRayHit.point - (BlockSelectorRayHit.normal * 0.5f);
            Vector3 roundedPos = new Vector3(Mathf.RoundToInt(rawPos.x), Mathf.RoundToInt(rawPos.y), Mathf.RoundToInt(rawPos.z));
            this.BlockSelector.transform.position = roundedPos;
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
            if(Physics.Raycast(ray, out RaycastHit hit, MaxInteractableDistance))
            {
                Vector3 rawPos = hit.point + (hit.normal * 0.5f);
                Vector3 roundedPos = new Vector3(Mathf.RoundToInt(rawPos.x), Mathf.RoundToInt(rawPos.y), Mathf.RoundToInt(rawPos.z));
                MathHelper.AddBlock(roundedPos, Block.Grass);
            }
        }

        // On Left Click Down Remove Block (set to Air)
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.Raycast(ray, out RaycastHit hit, MaxInteractableDistance))
            {
                Vector3 rawPos = hit.point - (hit.normal * 0.5f);
                Vector3 roundedPos = new Vector3(Mathf.RoundToInt(rawPos.x), Mathf.RoundToInt(rawPos.y), Mathf.RoundToInt(rawPos.z));
                MathHelper.AddBlock(roundedPos, Block.Air);
            }
        }
    }
}
