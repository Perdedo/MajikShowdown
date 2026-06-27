using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static void SaveConfig(ConfigData data)
    {
        string s = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/configSave.json", s);
    }

    public static ConfigData LoadConfig(ref bool success)
    {
        string path = Application.persistentDataPath + "/configSave.json";
        if (!File.Exists(path))
        {
            success = false;
            return new ConfigData(0, 0, 0f, -15f, -15f, false);
        }
        string s = File.ReadAllText(path);
        return JsonUtility.FromJson<ConfigData>(s);
    }
}
