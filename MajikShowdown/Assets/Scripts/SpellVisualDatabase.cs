using System.Collections.Generic;
using UnityEngine;

public class SpellVisualDatabase : MonoBehaviour
{
    public static SpellVisualDatabase Instance;

    public List<Sprite> icons;

    public Color[] colors =
    {
        Color.white,
        Color.black,
        Color.red,
        new Color(1f, 0.5f, 0f),
        Color.yellow,
        Color.green,
        Color.blue,
        new Color(0.7f, 0f, 1f)
    };

    private void Awake()
    {
        Instance = this;
    }
}