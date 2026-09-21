using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class BufferTests : MonoBehaviour
{
    [SerializeField]private List<GameObject> points = new List<GameObject>();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            TesteElement(VfxElement.Lighting, points);

        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            TesteType(VfxType.Area,points);
        }
    }
    public void TesteElement(VfxElement ele, List<GameObject> points)
    {
        
        BuffersControl.Instance.SpawnEffect(ele,VfxType.Projectile, points[0].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,VfxType.Projectile, points[1].transform, 2);
        BuffersControl.Instance.SpawnEffect(ele,VfxType.Explosion, points[2].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,VfxType.Explosion, points[3].transform, 2);
        BuffersControl.Instance.SpawnEffect(ele,VfxType.Area, points[4].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,VfxType.Area, points[5].transform, 2);
        
    }
    public void TesteType(VfxType type, List<GameObject> points)
    {

        BuffersControl.Instance.SpawnEffect(VfxElement.Fire,type, points[0].transform, 1);
        BuffersControl.Instance.SpawnEffect(VfxElement.Radiance,type, points[1].transform, 1);
        BuffersControl.Instance.SpawnEffect(VfxElement.Darkness,type, points[2].transform, 1);
        BuffersControl.Instance.SpawnEffect(VfxElement.Ice,type, points[3].transform, 1);
        BuffersControl.Instance.SpawnEffect(VfxElement.Earth,type, points[4].transform, 1);
        BuffersControl.Instance.SpawnEffect(VfxElement.Poison,type, points[5].transform, 1);
        
    }
}
