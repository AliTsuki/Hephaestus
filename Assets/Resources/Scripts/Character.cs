using UnityEngine;

namespace OLD
{
    // Class for Character Controller
    public class Character : MonoBehaviour
    {
        // Character fields
        public GameObject BlockSelector;
        public static float MaxInteractableDistance = 8f;

        // Start is called before the first frame update
        // Character Start
        private void Start()
        {

        }

        // Update is called once per frame
        // Character Update: Check Mouse clicks and Add or Delete Blocks
        // TODO: Limit placing blocks to not be inside player getting stuck, Add code for accepting tool and blocktype for adding/deleting
        private void Update()
        {
            // Block Selector Cube
            Ray blockSelectorRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.Raycast(blockSelectorRay, out RaycastHit blockSelectorRayHit, MaxInteractableDistance))
            {
                this.BlockSelector.SetActive(true);
                this.BlockSelector.transform.GetComponent<MeshRenderer>().enabled = true;
                Vector3 rawPos = blockSelectorRayHit.point - (blockSelectorRayHit.normal * 0.5f);
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
                    AddBlock(roundedPos, Block.Grass);
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
                    AddBlock(roundedPos, Block.Air);
                }
            }
        }

        // Add Block to Chunk, used by Player, given World Coords as Vector3 and Block
        private static void AddBlock(Vector3 _position, Block _block)
        {
            try
            {
                Int3 chunkPos = new Int3(_position);
                chunkPos.WorldCoordsToChunkCoords();
                Chunk currentChunk = World.Instance.GetChunk(chunkPos);
                if(currentChunk.GetType().Equals(typeof(ErroredChunk)))
                {
                    Debug.Log($@"{GameManager.Time}: Current CHUNK is ERRORED: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
                    return;
                }
                else
                {
                    Int3 pos = new Int3(_position);
                    pos.WorldCoordsToInternalChunkCoords();
                    currentChunk.SetBlockPlayer(pos, _block);
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
        }
    }
}
