using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class VFXNetworkAuxiliar : NetworkBehaviour
{
    public List<SetGraphicsBuffer> vfxBuffers = new List<SetGraphicsBuffer>();
    public static VFXNetworkAuxiliar instance;

    public override void OnStartClient()
    {
        instance = this;
        foreach(SetGraphicsBuffer buffer in vfxBuffers)
        {
            buffer.Initialize();
        }
    }

    private void LateUpdate()
    {
        if(isServer)
        {
            return;
        }
        foreach (SetGraphicsBuffer buffer in vfxBuffers)
        {
            buffer.ClientUpdateVFXs();
        }
    }

    public void DisableBuffer(SetGraphicsBuffer buffer)
    {
        if(!isServer)
        {
            return;
        }
        RPCDisableBuffer(vfxBuffers.IndexOf(buffer));
    }

    [ClientRpc]
    public void RPCDisableBuffer(int index)
    {
        if (isServer)
        {
            return;
        }
        vfxBuffers[index].ClientDisableBuffer();
    }

    public void AddEffect(SetGraphicsBuffer buffer, Transform place, float size)
    {
        if(!isServer)
        {
            return;
        }
        RPCAddEffect(vfxBuffers.IndexOf(buffer), place.gameObject, size);
    }

    [ClientRpc]
    public void RPCAddEffect(int index, GameObject place, float size)
    {
        if(isServer)
        {
            return;
        }
        vfxBuffers[index].ClientAddEffect(place.transform, size);
    }

    public void RemoveEffect(SetGraphicsBuffer buffer, int index)
    {
        if(!isServer)
        {
            return;
        }
        RPCRemoveEffect(vfxBuffers.IndexOf(buffer), index);
    }

    [ClientRpc]
    public void RPCRemoveEffect(int bufferIndex, int index)
    {
        if (isServer)
        {
            return;
        }
        vfxBuffers[bufferIndex].ClientRemoveEffect(index);
    }
}
