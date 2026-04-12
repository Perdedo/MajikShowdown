using System;
using UnityEngine;

[Serializable]
public class SimpleVar
{
    public enum ValueType { Fixed, Random, Infinity }
    [SerializeField] public ValueType type;
}
[Serializable]
public class SimpleInt : SimpleVar
{
    public int value;
    public int min;
    public int max;

    public virtual int GetValue()
    {
        return GetBaseValue();
    }
    protected int GetBaseValue()
    {
        switch (type)
        {
            case ValueType.Fixed:
                return value;
            case ValueType.Random:
                return UnityEngine.Random.Range(min, max + 1);
            case ValueType.Infinity:
                return int.MaxValue;
            default: return 0;
        }
    }
}
[Serializable]
public class SimpleFloat : SimpleVar
{
    public float value;
    public float min;
    public float max;

    public virtual float GetValue()
    {
        return GetBaseValue();
    }
    protected float GetBaseValue()
    {
        switch (type)
        {
            case ValueType.Fixed:
                return value;
            case ValueType.Random:
                return UnityEngine.Random.Range(min, max);
            case ValueType.Infinity:
                return Mathf.Infinity;
            default: return 0;
        }
    }
}