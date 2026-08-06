using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
public class SetGraphicsBuffer : MonoBehaviour
{
    private const int STRIDE = 12;

    [SerializeField] private int bufferCapacity = 8;
    
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private ExposedProperty bufferPoints = "SpawnPoints";
    [SerializeField] private ExposedProperty bufferCount = "PointsCount";

    public List<Vector3> spawnPoints = new List<Vector3>();
    private GraphicsBuffer gBuffer;

    void Awake()
    {
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
    }

    void LateUpdate()
    {
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
        gBuffer.SetData(spawnPoints);
        visualEffect.SetInt(bufferCount, spawnPoints.Count);


    }
    
    void OnDestroy()
    {
        ReleaseBuffer(ref gBuffer);
    }

    private void EnsureBufferCap(ref GraphicsBuffer buffer, int capacity, int stride, VisualEffect vfx, int vfxBufferProperty)
    {
        if(buffer == null || buffer.count < capacity)
        { 
            Debug.Log("aQUI");
            buffer?.Release();

            buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, stride);

            vfx.SetGraphicsBuffer(vfxBufferProperty, buffer);
        }
        

    }
     private void ReleaseBuffer(ref GraphicsBuffer buffer)
    {
        buffer?.Release();
        buffer = null;
    }
}
