using UnityEngine;


/// <summary>
/// Class contains player state data.
/// </summary>
public class Player : MonoBehaviour
{
    public uint ID;
    public string Name = "Player";
    public bool IsAlive = true;
    public float HealthCurrent = 200.0f;
    public float HealthMax = 200.0f;
    protected AudioSource AudioSrc;
    public PlayerController PC { get; protected set; }


    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        this.PC = this.GetComponent<PlayerController>();
        this.AudioSrc = this.GetComponent<AudioSource>();
        this.HealthCurrent = this.HealthMax;
    }

    // Start is called before the first frame update.
    private void Start()
    {
        
    }

    // FixedUpdate is called a fixed number of times per second.
    private void FixedUpdate()
    {
        
    }
}
