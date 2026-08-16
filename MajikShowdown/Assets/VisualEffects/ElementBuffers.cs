using System.Collections.Generic;
using UnityEngine;

public class ElementBuffers : MonoBehaviour
{
    public VfxElement myElement;
    [SerializeField] private List<SetGraphicsBuffer> mybuffers;

    public void CallBuffer(VfxType type, Transform place, float size)
    {
        for(int i = 0; i < mybuffers.Count; i++)
        {
            if(mybuffers[i].myType == type)
            {
                mybuffers[i].AddEffect(place,size);
                i = mybuffers.Count;
            }else if(i == mybuffers.Count - 1)
            {
                Debug.LogError("VFX type don't exist");
            }
            
        }
    }
}
