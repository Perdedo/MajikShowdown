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
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            /*SpawnEffect(VfxElement.Fire,VfxType.Projectile, buffers[0].transform, 1);
            SpawnEffect(VfxElement.Radiance,VfxType.Projectile, buffers[1].transform, 1);
            SpawnEffect(VfxElement.Darkness,VfxType.Projectile, buffers[2].transform, 1);
            SpawnEffect(VfxElement.Ice,VfxType.Projectile, buffers[3].transform, 1);
            SpawnEffect(VfxElement.Earth,VfxType.Projectile, buffers[4].transform, 1);*/
            SpawnEffect(VfxElement.Poison,VfxType.Projectile, buffers[5].transform, 1);

        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SpawnEffect(VfxElement.Fire,VfxType.Explosion, buffers[0].transform, 1);
            SpawnEffect(VfxElement.Radiance,VfxType.Explosion, buffers[1].transform, 1);
            SpawnEffect(VfxElement.Darkness,VfxType.Explosion, buffers[2].transform, 1);
            SpawnEffect(VfxElement.Ice,VfxType.Explosion, buffers[3].transform, 1);
            SpawnEffect(VfxElement.Earth,VfxType.Explosion, buffers[4].transform, 1);
            SpawnEffect(VfxElement.Poison,VfxType.Explosion, buffers[5].transform, 1);
        }
    }
    public int SpawnEffect(VfxElement element, VfxType type, Transform place, float size)
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
    public void UnspawnEffect(VfxElement element, VfxType type, int index)
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
