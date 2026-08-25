using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ConfigData
{
    public int selectedRes, screenMode, antiAliasing;
    public float master, music, sfx;
    public bool vsyncEnabled;
    public ConfigData(int selectedRes, int screenMode, int antiAliasing, float master, float music, float sfx, bool vSync = false)
    {
        this.selectedRes = selectedRes;
        this.screenMode = screenMode;
        this.antiAliasing = antiAliasing;
        this.master = master;
        this.music = music;
        this.sfx = sfx;
        this.vsyncEnabled = vSync;
    }
}
