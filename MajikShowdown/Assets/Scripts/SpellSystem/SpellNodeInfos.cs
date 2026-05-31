using System;
using UnityEngine;

[Serializable]
public struct NodeVisualInfo
{
    public Color color;
    public Color internSymbolColor;
    public Sprite borderSprite;
    public NodeConection.Conections[] connections;
}

[CreateAssetMenu(fileName = "SpellNodeInfos", menuName = "Scriptable Objects/SpellNodeInfos")]
public class SpellNodeInfos : ScriptableObject
{
    [Header("Node Types")]
    public NodeVisualInfo core;

    public NodeVisualInfo effect;

    public NodeVisualInfo trajectory;

    public NodeVisualInfo stat;

    public NodeVisualInfo trigger;

    public NodeVisualInfo castingPoint;

    public NodeVisualInfo GetInfo(NodeCategory category)
    {
        switch (category)
        {
            case NodeCategory.Type:
                return core;

            case NodeCategory.Effect:
                return effect;

            case NodeCategory.Trajectory:
                return trajectory;

            case NodeCategory.Stat:
                return stat;

            case NodeCategory.Trigger:
                return trigger;

            /*case NodeCategory.CastingPoint:
                return castingPoint;*/

            default:
                return core;
        }
    }
}
