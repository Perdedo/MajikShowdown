using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowFieldAsset", menuName = "Scriptable Objects/FlowFieldAsset")]
public class FlowFieldAsset : ScriptableObject
{
    public List<FlowFieldDivision> fieldAsset = new List<FlowFieldDivision>();
}
