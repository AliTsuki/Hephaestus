using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interface for Loopables
public interface ILoopable
{
    void Start();
    void Update();
    void OnApplicationQuit();
}
