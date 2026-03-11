using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static void SaveConfig()
    {
        string s = JsonUtility.ToJson(GameManager.Instance.uiController.data);
        File.WriteAllText(Application.persistentDataPath + "/configSave.json", s);
    }

    public static void LoadConfig()
    {
        string path = Application.persistentDataPath + "/configSave.json";
        if (!File.Exists(path)) return;
        string s = File.ReadAllText(path);
        GameManager.Instance.uiController.data = JsonUtility.FromJson<ConfigData>(s);
    }
}
