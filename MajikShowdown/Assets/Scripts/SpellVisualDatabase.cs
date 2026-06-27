using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Spell Visual Database")]
public class SpellVisualDatabase : ScriptableObject
{
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
}