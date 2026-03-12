using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SpellNode : ScriptableObject
{
    public float Cooldown = 0;
    public StatTypes BaseStats = new StatTypes();
    public SpellNodeInterface Interface;
    public int hierarchy = int.MaxValue;
    //public NodeConection[] conections;
    public SpellNode[] ConectedNodes = new SpellNode[6];
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
        //conections = new NodeConection[]{new(this), new(this), new(this),new(this), new(this), new(this)};
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
    public enum Conections { Circle, Triangle, Square, Penta, None, All }
    public Conections conectionType = Conections.None;
    public NodeConection conection;
    public bool TryConect(NodeConection c)
    {
        if (conection == null && c.conectionType == conectionType)
        {
            c.conection = this;
            conection = c;
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
    public void RemoveConection()
    {
        if(conection != null)
        {
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
}
