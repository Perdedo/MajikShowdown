using UnityEngine;

[CreateAssetMenu(fileName = "SpellNodeInfos", menuName = "Scriptable Objects/SpellNodeInfos")]
public class SpellNodeInfos : ScriptableObject
{
    [Header("Borders")]
    public Sprite coreBorder;

    public Sprite effectBorder;

    public Sprite trajectoryBorder;

    public Sprite statBorder;

    public Sprite triggerBorder;

    public Sprite castingPointBorder;
}
