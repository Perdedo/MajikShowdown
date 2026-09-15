using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "RuneLootPool", menuName = "Scriptable Objects/RuneLootPool")]
public class RuneLootPool : ScriptableObject
{
    public ProbabilitySlider<SpellNode.Quality> RarityChance = new ProbabilitySlider<SpellNode.Quality>(new List<(string label, float weight, SpellNode.Quality value)>
    {
        ("Rusty", 0.6f, SpellNode.Quality.Rusty),
        ("Forged", 0.3f, SpellNode.Quality.Forged),
        ("FactoryNew", 0.1f, SpellNode.Quality.FactoryNew)/*,
        ("Epic", 0.04f, SpellNode.Quality.Epic),
        ("Legendary", 0.01f, SpellNode.Quality.Legendary)*/
    });
    public RuneQualityGroup Rusty = new RuneQualityGroup();
    public RuneQualityGroup Forged = new RuneQualityGroup();
    public RuneQualityGroup FactoryNew = new RuneQualityGroup();
    //public RuneQualityGroup Epic = new RuneQualityGroup();
    //public RuneQualityGroup Legendary = new RuneQualityGroup();

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
            case SpellNode.Quality.Rusty:
                node = Rusty.GetRandomNode();
                break;
            case SpellNode.Quality.Forged:
                node = Forged.GetRandomNode();
                break;
            case SpellNode.Quality.FactoryNew:
                node = FactoryNew.GetRandomNode();
                break;
            /*case SpellNode.Quality.Epic:
                node = Epic.GetRandomNode();
                break;
            case SpellNode.Quality.Legendary:
                node = Legendary.GetRandomNode();
                break;*/
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
public class RuneQualityGroup
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
    public List<SpellCore> Core = new List<SpellCore>();
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
