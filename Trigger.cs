using UnityEngine;

public class Trigger : MonoBehaviour
{
    public TriggerHolder holder;
    void Start()
    {
        var th = VoxLib.ModLib.GlobalSudo.GetCache();
        holder = new TriggerHolder()
        {
            AirLayer = th.AirLayer,
            GrassLayer = th.GrassLayer,
            DirtLayer = th.DirtLayer,
            StoneLayer = th.StoneLayer,
            BedrockLayer = th.BedrockLayer,
            PerlinDivision = th.PerlinDivision,
            PerlinMultiplier = th.PerlinMultiplier,
            YMul = th.YMul
        };
    }

    void Update()
    {
        var th = new VoxLib.ModLib.TriggerHolder()
        {
            AirLayer = holder.AirLayer,
            GrassLayer = holder.GrassLayer,
            DirtLayer = holder.DirtLayer,
            StoneLayer = holder.StoneLayer,
            BedrockLayer = holder.BedrockLayer,
            PerlinDivision = holder.PerlinDivision,
            PerlinMultiplier = holder.PerlinMultiplier,
            YMul = holder.YMul
        };
        VoxLib.ModLib.GlobalSudo.SetCache(th);
    }
}
