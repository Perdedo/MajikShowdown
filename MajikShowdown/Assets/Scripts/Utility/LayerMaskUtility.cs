using UnityEngine;

public class LayerMaskUtility
{
    public static bool BelongsInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
