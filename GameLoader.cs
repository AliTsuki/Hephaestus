using System;
using System.Linq;

using Assets.ModLib;

using UnityEngine;

public class GameLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(var item in System.IO.Directory.GetFiles("Mods").Where(o => o.EndsWith(".dll")))
        {
            AppDomain.CurrentDomain.Load(System.IO.File.ReadAllBytes(item));
        }
        var mloader = new SimpleModLoader();
        mloader.OnGameLoad();
        mloader.Start();
    }

    // Update is called once per frame
    void Update()
    {
        SimpleModLoader.GetModLoader().Tick();
    }
}
