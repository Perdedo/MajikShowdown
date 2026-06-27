using System;
using UnityEngine;

[Serializable]
public class SimpleVar
{
    public enum ValueType { Fixed, Random, Infinity, CurveRandom }
    [SerializeField] public ValueType type;
}
[Serializable]
public class SimpleInt : SimpleVar
{
    public int value;
    public int min;
    public int max;
    public AnimationCurve curve;
    [Tooltip("A multiplier applied to the final value obtained from the curve.")]
    public float curveMultiplier = 1f;
    [Tooltip("The max X value in the curve that will be used for random evaluation.")]
    public float curveTimeMax = 1;

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
            case ValueType.CurveRandom:
                float t = UnityEngine.Random.value * curveTimeMax;
                return Mathf.RoundToInt(curve.Evaluate(t) * curveMultiplier);
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
    public AnimationCurve curve;
    [Tooltip("A multiplier applied to the final value obtained from the curve.")]
    public float curveMultiplier = 1f;
    [Tooltip("The max X value in the curve that will be used for random evaluation.")]
    public float curveTimeMax = 1;

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
            case ValueType.CurveRandom:
                float t = UnityEngine.Random.value * curveTimeMax;
                return curve.Evaluate(t) * curveMultiplier;
            default: return 0;
        }
    }
}