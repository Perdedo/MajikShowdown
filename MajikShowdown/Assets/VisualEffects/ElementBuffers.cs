using System.Collections.Generic;
using UnityEngine;

public class ElementBuffers : MonoBehaviour
{
    public VfxElement myElement;
    [SerializeField] private List<SetGraphicsBuffer> mybuffers;

    public int CallBuffer(VfxType type, Transform place, float size)
    {
        int index = -1;
        for(int i = 0; i < mybuffers.Count; i++)
        {
            if(mybuffers[i].myType == type)
            {
                index = mybuffers[i].AddEffect(place,size);
                i = mybuffers.Count;
            
            }else if(i == mybuffers.Count - 1)
            {
                Debug.LogError("VFX type don't exist");
            }
            
        }
        return index;
    }
    public void UnspawnVfx( VfxType type, int index)
    {
        for(int i = 0; i < mybuffers.Count; i++)
        {
            if(mybuffers[i].myType == type)
            {
                mybuffers[i].RemoveEffect(index);
                i = mybuffers.Count;
            }
        }
    }
}
