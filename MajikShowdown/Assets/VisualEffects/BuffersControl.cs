using System.Collections.Generic;
using System;
using UnityEngine;

public enum VfxElement
{
    Fire, Ice, Poison, Darkness, Earth, Radiance
}
public enum VfxType
{
    Projectile, Explosion, Area
}
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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            SpawnEffect(VfxElement.Radiance,VfxType.Area, transform, 1);
        }
    }
    public void SpawnEffect(VfxElement element, VfxType type, Transform place, float size)
    {
        for(int i = 0; i < buffers.Count; i++)
        {
            if(buffers[i].myElement == element)
            {
                buffers[i].CallBuffer(type, place, size);
            }
        }
    }
}
