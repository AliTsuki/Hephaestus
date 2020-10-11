using TMPro;

using UnityEngine;


/// <summary>
/// 
/// </summary>
public class UIController : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public TextMeshProUGUI DebugText;
    /// <summary>
    /// 
    /// </summary>
    private const string na = "N/A";


    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        this.DebugText = this.GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update.
    private void Start()
    {
        
    }

    // Update is called once per frame.
    private void Update()
    {
        if(GameManager.Instance.Player != null)
        {
            this.DebugText.text = $@"PLAYER----------------
Pos: ({GameManager.Instance.Player.transform.position})
Rounded: ({GameManager.Instance.Player.transform.position.RoundToInt()})
Chunk Pos: ({GameManager.Instance.Player.transform.position.RoundToInt().WorldPosToChunkPos()})
SELECTION-------------
Pos: ({(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.BlockSelectorHit.point.ToString() : na)})
Face: ({(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.BlockSelectorHit.normal.ToString() : na)})
Rounded: ({(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.CurrentBlockSelectedPos.ToString() : na)})
Chunk: ({(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.CurrentBlockSelectedPos.WorldPosToChunkPos().ToString() : na)})
Block: {(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.CurrentBlockSelected.BlockName : na)}";
        }
    }
}
