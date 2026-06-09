using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "RuneLootPool", menuName = "Scriptable Objects/RuneLootPool")]
public class RuneLootPool : ScriptableObject
{
    public ProbabilitySlider<SpellNode.Rarity> RarityChance = new ProbabilitySlider<SpellNode.Rarity>(new List<(string label, float weight, SpellNode.Rarity value)>
    {
        ("Common", 0.5f, SpellNode.Rarity.Common),
        ("Uncommon", 0.3f, SpellNode.Rarity.Uncommon),
        ("Rare", 0.15f, SpellNode.Rarity.Rare),
        ("Epic", 0.04f, SpellNode.Rarity.Epic),
        ("Legendary", 0.01f, SpellNode.Rarity.Legendary)
    });
    public RuneRaretyGroup Common = new RuneRaretyGroup();
    public RuneRaretyGroup Uncommon = new RuneRaretyGroup();
    public RuneRaretyGroup Rare = new RuneRaretyGroup();
    public RuneRaretyGroup Epic = new RuneRaretyGroup();
    public RuneRaretyGroup Legendary = new RuneRaretyGroup();

    [ContextMenu("Test Probabilities")]
    public void TestProb()
    {
        foreach (var entry in RarityChance.Entries)
        {
            Debug.Log($"{entry.label}: {entry.Weight}");
        }
    }
    public SpellNode GetLoot()
    {
        SpellNode node;
        switch (RarityChance.GetRandomEntry())
        {
            case SpellNode.Rarity.Common:
                node = Common.GetRandomNode();
                break;
            case SpellNode.Rarity.Uncommon:
                node = Uncommon.GetRandomNode();
                break;
            case SpellNode.Rarity.Rare:
                node = Rare.GetRandomNode();
                break;
            case SpellNode.Rarity.Epic:
                node = Epic.GetRandomNode();
                break;
            case SpellNode.Rarity.Legendary:
                node = Legendary.GetRandomNode();
                break;
            default:
                node = null;
                break;
        }
        if(node == null)
        {
            Debug.LogError("No node found");
        }
        return node;
    }
}
[Serializable]
public class RuneRaretyGroup
{
    public ProbabilitySlider<NodeType> TypeChance = new ProbabilitySlider<NodeType>(new List<(string label, float weight, NodeType value)>
    {
        ("Core", 0.2f, NodeType.Core),
        ("Trajectory", 0.2f, NodeType.Trajectory),
        ("Effect", 0.2f, NodeType.Effect),
        ("Stat", 0.2f, NodeType.Stat),
        ("Trigger", 0.1f, NodeType.Trigger),
        ("CastPoint", 0.1f, NodeType.CastPoint)
    });
    public List<SpellType> Core = new List<SpellType>();
    public List<SpellTrajectory> Trajectory = new List<SpellTrajectory>();
    public List<SpellEffect> Effect = new List<SpellEffect>();
    public List<SpellStat> Stat = new List<SpellStat>();
    public List<SpellTrigger> Trigger = new List<SpellTrigger>();
    public List<SpellCastPoint> CastPoint = new List<SpellCastPoint>();
    public SpellNode GetRandomNode()
    {
        List<SpellNode> nodeList;
        switch (TypeChance.GetRandomEntry())        {
            case NodeType.Core:
                nodeList = Core.ConvertAll(n => (SpellNode)n);
                break;
            case NodeType.Trajectory:
                nodeList = Trajectory.ConvertAll(n => (SpellNode)n);
                break;
            case NodeType.Effect:
                nodeList = Effect.ConvertAll(n => (SpellNode)n);
                break;
            case NodeType.Stat:
                nodeList = Stat.ConvertAll(n => (SpellNode)n);
                break;
            case NodeType.Trigger:
                nodeList = Trigger.ConvertAll(n => (SpellNode)n);
                break;
            case NodeType.CastPoint:
                nodeList = CastPoint.ConvertAll(n => (SpellNode)n);
                break;
            default:
                return null;
        }
        return nodeList[UnityEngine.Random.Range(0, nodeList.Count)];
    }
    public List<SpellNode> GetAllNodes()
    {
        List<SpellNode> allNodes = new List<SpellNode>();
        allNodes.AddRange(Core);
        allNodes.AddRange(Trajectory);
        allNodes.AddRange(Effect);
        allNodes.AddRange(Stat);
        allNodes.AddRange(Trigger);
        allNodes.AddRange(CastPoint);
        return allNodes;
    }
    public enum NodeType { Core, Trajectory, Effect, Stat, Trigger, CastPoint }
}
