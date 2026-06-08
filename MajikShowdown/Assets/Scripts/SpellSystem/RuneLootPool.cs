using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RuneLootPool", menuName = "Scriptable Objects/RuneLootPool")]
public class RuneLootPool : ScriptableObject
{
    public ProbabilitySlider<List<SpellNode>> RarityChance = new ProbabilitySlider<List<SpellNode>>(new List<(string label, float weight, List<SpellNode> value)>
    {
        ("Common", 0.5f, new List<SpellNode>()),
        ("Uncommon", 0.3f, new List<SpellNode>()),
        ("Rare", 0.15f, new List<SpellNode>()),
        ("Epic", 0.04f, new List<SpellNode>()),
        ("Legendary", 0.01f, new List<SpellNode>())
    });
    [ContextMenu("Test Probabilities")]
    public void TestProb()
    {
        foreach (var entry in RarityChance.Entries)
        {
            Debug.Log($"{entry.label}: {entry.Weight}");
        }
    }
}
