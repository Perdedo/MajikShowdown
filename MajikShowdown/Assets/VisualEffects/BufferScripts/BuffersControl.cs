using System.Collections.Generic;
using System;
using UnityEngine;

public class BuffersControl : MonoBehaviour
{    
    public static BuffersControl Instance;
    [SerializeField] private List<ElementBuffers> buffers = new List<ElementBuffers>();
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public int SpawnEffect(Elements element, SpellTypes type, Transform place, float size)
    {
        if(type != SpellTypes.Explosion)
        {
            int index = -1;
            for(int i = 0; i < buffers.Count; i++)
            {
                if(buffers[i].myElement == element)
                {
                    index = buffers[i].CallBuffer(type, place, size);
                    i = buffers.Count;

                }
                else if(i == buffers.Count - 1)
                {
                    Debug.LogError("VFX element don't exist");
                }
            }
            return index;
        }
        else
        {
            Debug.LogError("Wrong function for explosion");
            return 0;
        }
       
    }
    public int SpawnEffect(Elements element, SpellTypes type, Transform place, float size, float time)
    {
        int index = -1;
        size = size + (time/100);
        for(int i = 0; i < buffers.Count; i++)
        {
            if(buffers[i].myElement == element)
            {
                index = buffers[i].CallBuffer(type, place, size);
                i = buffers.Count;

            }
            else if(i == buffers.Count - 1)
            {
                Debug.LogError("VFX element don't exist");
            }
        }
        return index;
    }
    public void UnspawnEffect(Elements element, SpellTypes type, int index)
    {
        for(int i = 0; i < buffers.Count; i++)
        {
            if(buffers[i].myElement == element)
            {
                buffers[i].UnspawnVfx(type, index);
                i = buffers.Count;
            }
        }
    }
}
