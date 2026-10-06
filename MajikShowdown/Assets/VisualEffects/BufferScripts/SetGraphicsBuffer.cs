using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using System.Collections.Generic;
using System;
using Steamworks;

public class SetGraphicsBuffer : MonoBehaviour
{
    private const  int STRIDE = 16;
    public SpellTypes myType;
    public List<Vector4> spawnPoints = new List<Vector4>();
    private GraphicsBuffer gBuffer;
    [SerializeField] private int bufferCapacity = 8;    
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private ExposedProperty bufferPoints = "SpawnPoints";
    [SerializeField] private ExposedProperty bufferCount = "PointsCount";
    [SerializeField] private List<Transform> instances = new List<Transform>();
    [SerializeField] private Queue<int> nullIndex = new Queue<int>();
    static readonly ExposedProperty newEvent = "OnNewExplosion";
    bool newCall = false;
    
    void Start()
    {
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
    }
    public int AddEffect(Transform place, float size)
    {
        if(myType != SpellTypes.Explosion)
        {
            int index = instances.Count;
            if(nullIndex.Count == 0)
            {
                instances.Add(place);
                spawnPoints.Add(new Vector4(place.position.x,place.position.y,place.position.z,size));
            }
            else
            {
                index = nullIndex.Dequeue();
                instances[index] = place;
                spawnPoints[index] = new Vector4(place.position.x,place.position.y,place.position.z,size);
            }
            return index;
        }
        else
        {
            int index = instances.Count;
            if(nullIndex.Count == 0)
            {
                instances.Add(place);
                spawnPoints.Add(new Vector4(place.position.x,place.position.y,place.position.z,size));
                newCall = true;
            }
            else
            {
                index = nullIndex.Dequeue();
                instances[index] = place;
                spawnPoints[index] = new Vector4(place.position.x,place.position.y,place.position.z,size);
                newCall = true;
            }
            return index;
        }
        
    }
    public void RemoveEffect(int index)
    {
        instances[index] = null;
        nullIndex.Enqueue(index);    
    }
    void LateUpdate()
    {
        UpdatePoints();
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
        
        List<Vector4> points = GetBufferPoints();
        gBuffer.SetData(points);
        visualEffect.SetInt(bufferCount, points.Count);
        if (newCall)
        {
            visualEffect.SendEvent(newEvent);
            newCall = false;
        }
    }
    
    void OnDisable()
    {
        ReleaseBuffer(ref gBuffer);
    }

    private void EnsureBufferCap(ref GraphicsBuffer buffer, int capacity, int stride, VisualEffect vfx, int vfxBufferProperty)
    {
        if(buffer == null || buffer.count < capacity)
        { 
            buffer?.Release();

            buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, stride);

            vfx.SetGraphicsBuffer(vfxBufferProperty, buffer);
        }
        

    }

    private List<Vector4> GetBufferPoints()
    {
        List<Vector4> bPoints = new List<Vector4>();
        for(int i = 0; i< spawnPoints.Count; i++)
        {
            if (!nullIndex.Contains(i))
            {
                bPoints.Add(spawnPoints[i]);
            }
        }
        return bPoints;
    }
    private void UpdatePoints()
    {
        if(spawnPoints.Count > 0 && instances.Count > 0)
        {
            for(int i = 0; i< spawnPoints.Count; i++)
            {
                if(instances[i] != null)
                {
                    Vector4 newpos = new Vector4(instances[i].position.x,instances[i].position.y,instances[i].position.z,spawnPoints[i].w);
                    spawnPoints[i] = newpos;
                }
  
            }
        }
        
    }
    private void ReleaseBuffer(ref GraphicsBuffer buffer)
    {
        buffer?.Release();
        buffer = null;
    }
}
