using BepInEx;
using UnityEngine;
using UnityEngine.UI;

[BepInPlugin("com.yourname.wobblyac", "Wobbly AC", "1.0")]
public class WobblyAC : BaseUnityPlugin
{

    void Start()
    {
        Logger.LogInfo("UI created");
    }

    void Update()
    {
    }
}