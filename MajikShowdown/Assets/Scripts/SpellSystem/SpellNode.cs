using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SpellNode : ScriptableObject
{
    [Header("Define Stat Randomization")]
    public StatRandomizer statRandomizer;
    [Header("Display")]
    public string spellDescription;
    public Color color = Color.white;
    [Header("Final Stats Debug")]
    public float Cooldown = 0;
    public StatTypes BaseStats = new StatTypes();
    [Header("Debug")]
    public SpellNodeInterface Interface;
    public int hierarchy = -1;
    //public NodeConection[] conections;
    public SpellNode[] ConectedNodes = new SpellNode[6];
    public Spell OwnerSpell;
    //public enum NodeEntry { None, Type, Stat, Trigger, Trajectory, Effect, All };
    /*public bool TryConectNode(SpellNode con, int index)
    {
        int mirrorIndex = (index + 3) % 6;
        if (index < conections.Length)
        {
            if (conections[index].TryConect(con.conections[mirrorIndex]))
            {
                ConectedNodes[index] = con;
                return true;
            }
            
        }
        return false;
    }*/
    /*public void BreakConection(SpellNode node)
    {
        for (int i = 0; i < ConectedNodes.Length; i++)
        {
            if (ConectedNodes[i] == node)
            {
                ConectedNodes[i] = null;
                conections[i].RemoveConection();
            }
        }
    }*/
    /*public void BreakConection(int Index)
    {
        if(Index >= ConectedNodes.Length)
        {
            return;
        }
        ConectedNodes[Index] = null;
        conections[Index].RemoveConection();
    }*/
    public virtual void Initialize()
    {
        RandomizeStats();
        //conections = new NodeConection[]{new(this), new(this), new(this),new(this), new(this), new(this)};
    }
    public virtual List<SpellNode> GetSpellList(List<SpellNode> list)
    {
        list.Add(this);
        foreach (SpellNode conectedNode in ConectedNodes)
        {
            if (conectedNode != null && !list.Contains(conectedNode))
            {
                list = conectedNode.GetSpellList(list);
            }
        }
        return list;
    }
    public virtual void ResetNode()
    {
        hierarchy = -1;
        OwnerSpell = null;
    }
    public virtual void RandomizeStats()
    {
        Cooldown = statRandomizer.Cooldown.GetValue();
        BaseStats.Randomize(statRandomizer);
    }
    public static T RandomizeEnum<T>(string[] exceptions = null)
    {
        List<string> validNames = new List<string>(Enum.GetNames(typeof(T)));
        if (exceptions != null)
        {
            foreach (string exception in exceptions)
            {
                validNames.Remove(exception);
            }
        }
        string name = validNames[UnityEngine.Random.Range(0, validNames.Count)];
        return (T)Enum.Parse(typeof(T), name);
    }
}
[Serializable]
public class NodeConection
{
    public NodeConection(SpellNode owner)
    {
        ownerNode = owner;
    }
    public SpellNode ownerNode;
    public enum Conections { None, Circle, Triangle, Square, Penta, All }
    public Conections conectionType = Conections.None;
    public NodeConection conection;
    public SpellNode conectedNode;
    public bool TryConect(NodeConection c)
    {
        if (c.conectionType == conectionType && conectionType != Conections.None && c.conectionType != Conections.None)
        {
            c.conection = this;
            conection = c;
            c.conectedNode = ownerNode;
            conectedNode = c.ownerNode;
            if (conection.ownerNode.hierarchy > ownerNode.hierarchy)
            {
                conection.ownerNode.hierarchy = ownerNode.hierarchy + 1;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CheckConection(NodeConection c)
    {
        if (c.conectionType == conectionType /*&& conectionType != Conections.None && c.conectionType != Conections.None*/)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void RemoveConection()
    {
        if (conection != null)
        {
            conectedNode = null;
            conection.conectedNode = null;
            conection.conection = null;
            conection = null;
        }

    }
    public SpellNode GetNode()
    {
        if (conection == null)
        {
            return null;
        }
        return conection.ownerNode;
    }
}

[Serializable]
public struct StatTypes
{
    public StatTypes(float DefaultValue = 0)
    {
        Speed = DefaultValue;
        Duration = DefaultValue;
        Size = DefaultValue;
        Damage = DefaultValue;
        Piercing = DefaultValue;
        Bounce = DefaultValue;
        Knockback = DefaultValue;
    }
    public float Speed;
    public float Duration;
    public float Size;
    public float Damage;
    public float Piercing;
    public float Bounce;
    public float Knockback;

    public static StatTypes operator +(StatTypes s1, StatTypes s2)
    {
        return new StatTypes()
        {
            Speed = s1.Speed + s2.Speed,
            Duration = s1.Duration + s2.Duration,
            Size = s1.Size + s2.Size,
            Damage = s1.Damage + s2.Damage,
            Piercing = s1.Piercing + s2.Piercing,
            Bounce = s1.Bounce + s2.Bounce,
            Knockback = s1.Knockback + s2.Knockback
        };
    }
    public static StatTypes operator *(StatTypes s1, StatTypes s2)
    {
        return new StatTypes()
        {
            Speed = s1.Speed * s2.Speed,
            Duration = s1.Duration * s2.Duration,
            Size = s1.Size * s2.Size,
            Damage = s1.Damage * s2.Damage,
            Piercing = s1.Piercing * s2.Piercing,
            Bounce = s1.Bounce * s2.Bounce,
            Knockback = s1.Knockback * s2.Knockback
        };
    }
    public static StatTypes operator -(StatTypes s1, StatTypes s2)
    {
        return new StatTypes()
        {
            Speed = s1.Speed - s2.Speed,
            Duration = s1.Duration - s2.Duration,
            Size = s1.Size - s2.Size,
            Damage = s1.Damage - s2.Damage,
            Piercing = s1.Piercing - s2.Piercing,
            Bounce = s1.Bounce - s2.Bounce,
            Knockback = s1.Knockback - s2.Knockback
        };
    }
    public void Randomize(StatRandomizer randomizer)
    {
        Speed = randomizer.Speed.GetValue();
        Duration = randomizer.Duration.GetValue();
        Size = randomizer.Size.GetValue();
        Damage = randomizer.Damage.GetValue();
        Piercing = randomizer.Piercing.GetValue();
        Bounce = randomizer.Bounce.GetValue();
        Knockback = randomizer.Knockback.GetValue();
    }
}
[Serializable]
public struct StatRandomizer
{
    public SimpleFloat Cooldown;
    public SimpleFloat Speed;
    public SimpleFloat Duration;
    public SimpleFloat Size;
    public SimpleFloat Damage;
    public SimpleFloat Piercing;
    public SimpleFloat Bounce;
    public SimpleFloat Knockback;
}
