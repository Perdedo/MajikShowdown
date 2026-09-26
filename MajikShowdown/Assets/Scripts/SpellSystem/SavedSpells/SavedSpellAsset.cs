using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/Saved Spell")]
public class SavedSpellAsset : ScriptableObject
{
    public string spellName;
    public int colorIndex;
    public int symbolIndex;
    public List<SavedSpellNode> nodes = new List<SavedSpellNode>();
}

[Serializable]
public class SavedSpellNode
{
    public SpellNode node;
    public int gridIndex;
}