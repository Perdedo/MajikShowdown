using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using System.Collections.Generic;
using System;

public class SetGraphicsBuffer : MonoBehaviour
{
    private const  int STRIDE = 16;
    public VfxType myType;
    public List<Vector4> spawnPoints = new List<Vector4>();
    private GraphicsBuffer gBuffer;
    [SerializeField] private int bufferCapacity = 8;    
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private ExposedProperty bufferPoints = "SpawnPoints";
    [SerializeField] private ExposedProperty bufferCount = "PointsCount";
    [SerializeField] private List<Transform> instances = new List<Transform>();

    
    void Awake()
    {
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
    }
    public int AddEffect(Transform place, float size)
    {
        instances.Add(place);
        spawnPoints.Add(new Vector4(place.position.x,place.position.y,place.position.z,size));
        return instances.Count - 1;
    }
    public void RemoveEffect(int index)
    {
        if(instances.Count > index)
        {
            instances.RemoveAt(index);
            spawnPoints.RemoveAt(index);
        }
        else
        {
            Debug.LogError("Vfx is not spawned");
        }

    }
    void LateUpdate()
    {
        UpdatePoints();
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
        gBuffer.SetData(spawnPoints);
        visualEffect.SetInt(bufferCount, spawnPoints.Count);
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
    private void UpdatePoints()
    {
        for(int i = 0; i< spawnPoints.Count; i++)
        {
            Vector4 newpos = new Vector4(instances[i].position.x,instances[i].position.y,instances[i].position.z,spawnPoints[i].w);
            spawnPoints[i] = newpos;
        }
    }
    private void ReleaseBuffer(ref GraphicsBuffer buffer)
    {
        buffer?.Release();
        buffer = null;
    }
}
