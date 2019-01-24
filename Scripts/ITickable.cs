using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interface for Tickables
public interface ITickable
{
    void Tick();
    void Start();
    void Update();
    void OnUnityUpdate();
}
