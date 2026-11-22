using Steamworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

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
    static readonly ExposedProperty newEventExplosion = "OnNewExplosion";
    static readonly ExposedProperty newEventArea = "OnNewArea";
    static readonly ExposedProperty endEventArea = "OnEndArea";
    static readonly ExposedProperty positionAttribute = "position";
    static readonly ExposedProperty lifeAttribute = "lifetime";

    static readonly ExposedProperty indexAttribute = "MyIndex";

    static readonly ExposedProperty sizeAttribute = "size"; 
    static readonly ExposedProperty hitEventProjec = "OnHitProject";

    VFXEventAttribute eventAttribute;
    List<VFXEventAttribute> eventAttributes = new List<VFXEventAttribute>();
    ElementBuffers myParent;

    bool newCall = false;
    public VFXNetworkAuxiliar netAux;
    void Start()
    {
        if(netAux != null)
        {
            return;
        }
        eventAttribute = visualEffect.CreateVFXEventAttribute();
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
        myParent = this.transform.parent.GetComponent<ElementBuffers>();
    }

    public void Initialize()
    {
        eventAttribute = visualEffect.CreateVFXEventAttribute();
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);
        myParent = this.transform.parent.GetComponent<ElementBuffers>();
    }
    public int AddEffect(Transform place, float size)
    {
        if(netAux != null)
        {
            netAux.AddEffect(this, place, size);
        }
        if(myType != SpellTypes.Explosion && myType != SpellTypes.Area)
        {
            int index = instances.Count;
            if(nullIndex.Count == 0)
            {
                instances.Add(place);
                spawnPoints.Add(new Vector4(place.position.x,place.position.y,place.position.z,size));
                eventAttributes.Add(visualEffect.CreateVFXEventAttribute());
            }
            else
            {
                index = nullIndex.Dequeue();
                instances[index] = place;
                spawnPoints[index] = new Vector4(place.position.x,place.position.y,place.position.z,size);
            }
            if(myParent.myElement == Elements.Earth)
            {
                ExposedProperty newEvent = newEventArea;
                //eventAttribute.SetVector3(positionAttribute, place.position);
                //visualEffect.SendEvent(newEvent, eventAttribute);
                eventAttributes[index].SetVector3(positionAttribute, place.position);
                visualEffect.SendEvent(newEvent, eventAttributes[index]);

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
                eventAttributes.Add(visualEffect.CreateVFXEventAttribute());
            }
            else
            {
                index = nullIndex.Dequeue();
                instances[index] = place;
                spawnPoints[index] = new Vector4(place.position.x,place.position.y,place.position.z,size);
            }

            ExposedProperty newEvent = myType == SpellTypes.Explosion ? newEventExplosion : newEventArea;
            //eventAttribute.SetVector3(positionAttribute, place.position);
            //visualEffect.SendEvent(newEvent, eventAttribute);
            eventAttributes[index].SetVector3(positionAttribute, place.position);
            visualEffect.SendEvent(newEvent, eventAttributes[index]);
            return index;
        }
        
    }
    public int ClientAddEffect(Transform place, float size)
    {
        if (myType != SpellTypes.Explosion && myType != SpellTypes.Area)
        {
            int index = instances.Count;
            if (nullIndex.Count == 0)
            {
                instances.Add(place);
                spawnPoints.Add(new Vector4(place.position.x, place.position.y, place.position.z, size));
                eventAttributes.Add(visualEffect.CreateVFXEventAttribute());
            }
            else
            {
                index = nullIndex.Dequeue();
                instances[index] = place;
                spawnPoints[index] = new Vector4(place.position.x, place.position.y, place.position.z, size);
            }
            if (myParent.myElement == Elements.Earth)
            {
                ExposedProperty newEvent = newEventArea;
                //eventAttribute.SetVector3(positionAttribute, place.position);
                //visualEffect.SendEvent(newEvent, eventAttribute);
                eventAttributes[index].SetVector3(positionAttribute, place.position);
                visualEffect.SendEvent(newEvent, eventAttributes[index]);

            }
            return index;
        }
        else
        {
            int index = instances.Count;
            if (nullIndex.Count == 0)
            {
                instances.Add(place);
                spawnPoints.Add(new Vector4(place.position.x, place.position.y, place.position.z, size));
                eventAttributes.Add(visualEffect.CreateVFXEventAttribute());
            }
            else
            {
                index = nullIndex.Dequeue();
                instances[index] = place;
                spawnPoints[index] = new Vector4(place.position.x, place.position.y, place.position.z, size);
            }

            ExposedProperty newEvent = myType == SpellTypes.Explosion ? newEventExplosion : newEventArea;
            //eventAttribute.SetVector3(positionAttribute, place.position);
            //visualEffect.SendEvent(newEvent, eventAttribute);
            eventAttributes[index].SetVector3(positionAttribute, place.position);
            visualEffect.SendEvent(newEvent, eventAttributes[index]);
            return index;
        }

    }
    public void RemoveEffect(int index)
    {
        if (netAux != null)
        {
            netAux.RemoveEffect(this, index);
        }
        if(myType == SpellTypes.Area)
        {
            eventAttribute.SetVector3(positionAttribute, instances[index].position);
            eventAttribute.SetFloat(sizeAttribute, spawnPoints[index].w);
            visualEffect.SendEvent(endEventArea, eventAttribute);
            
        }else if(myType == SpellTypes.Projectile)
        {
            eventAttribute.SetVector3(positionAttribute, instances[index].position);
            eventAttribute.SetFloat(sizeAttribute, spawnPoints[index].w);
            visualEffect.SendEvent(hitEventProjec, eventAttribute);
        }

        instances[index] = null;
        nullIndex.Enqueue(index);    
    }
    public void ClientRemoveEffect(int index)
    {
        if (myType == SpellTypes.Area)
        {
            eventAttribute.SetVector3(positionAttribute, instances[index].position);
            eventAttribute.SetFloat(sizeAttribute, spawnPoints[index].w);
            visualEffect.SendEvent(endEventArea, eventAttribute);
        }
        else if (myType == SpellTypes.Projectile)
        {
            eventAttribute.SetVector3(positionAttribute, instances[index].position);
            eventAttribute.SetFloat(sizeAttribute, spawnPoints[index].w);
            visualEffect.SendEvent(hitEventProjec, eventAttribute);
        }

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
    }

    public void ClientUpdateVFXs()
    {
        UpdatePoints();
        EnsureBufferCap(ref gBuffer, bufferCapacity, STRIDE, visualEffect, bufferPoints);

        List<Vector4> points = GetBufferPoints();
        gBuffer.SetData(points);
        visualEffect.SetInt(bufferCount, points.Count);
    }
    
    void OnDisable()
    {
        ReleaseBuffer(ref gBuffer);
        if(netAux != null)
        {
            netAux.DisableBuffer(this);
        }
    }

    public void ClientDisableBuffer()
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
    Vector2 GetTimeSize(float dec)
    {
        float x = Mathf.Floor(dec * 10);
        float y =  100 * ((dec * 10) - x );
        Vector2 vec = new Vector2(x/10,y);
        return vec;
    }
}
