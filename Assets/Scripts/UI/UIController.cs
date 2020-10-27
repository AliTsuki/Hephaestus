using TMPro;

using UnityEngine;


/// <summary>
/// Controls the in game UI.
/// </summary>
public class UIController : MonoBehaviour
{
    /// <summary>
    /// The text of the in game debug window.
    /// </summary>
    public TextMeshProUGUI DebugText;
    /// <summary>
    /// Const string for "N/A".
    /// </summary>
    private const string na = "N/A";


    // Awake is called when the script instance is being loaded.
    private void Awake()
    {

    }

    // Start is called before the first frame update.
    private void Start()
    {
        
    }

    // Update is called once per frame.
    private void Update()
    {
        this.UpdateDebugWindow();
    }

    /// <summary>
    /// Updates the in game debug window with player and block selection coordinates/info.
    /// </summary>
    private void UpdateDebugWindow()
    {
       
        if(GameManager.Instance.Player != null)
        {
            Vector3Int forward = new Vector3(GameManager.Instance.Player.transform.forward.x, 0, GameManager.Instance.Player.transform.forward.z).normalized.RoundToInt();
            Vector2Int forwardNormal = new Vector2Int(forward.x, forward.z);
            this.DebugText.text = $@"PLAYER----------------
Pos: {GameManager.Instance.Player.transform.position}
Rounded: {GameManager.Instance.Player.transform.position.RoundToInt()}
Chunk: {GameManager.Instance.Player.transform.position.RoundToInt().WorldPosToChunkPos()}
Forward: {GameManager.Instance.Player.transform.forward}
Forward Rounded: {forward}
Forward Vec2: {forwardNormal}
SELECTION-------------
Pos: {(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.BlockSelectorHit.point.ToString() : na)}
Face: {(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.BlockSelectorHit.normal.ToString() : na)}
Rounded: {(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.CurrentBlockSelectedPos.ToString() : na)}
Chunk: {(GameManager.Instance.Player.PC.IsBlockSelected ? GameManager.Instance.Player.PC.CurrentBlockSelectedPos.WorldPosToChunkPos().ToString() : na)}
Block: {(GameManager.Instance.Player.PC.IsBlockSelected ? BlockType.BlockTypes[GameManager.Instance.Player.PC.CurrentBlockSelected.ID].BlockName : na)}
INVENTORY-------------
Block: {BlockType.BlockTypes[GameManager.Instance.Player.PC.BlockToPlace.ID].BlockName}";
        }
    }
}
