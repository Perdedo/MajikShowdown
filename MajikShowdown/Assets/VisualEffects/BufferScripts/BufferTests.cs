using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;

public class BufferTests : MonoBehaviour
{
    [SerializeField]private List<GameObject> points = new List<GameObject>();
    public int index = 2;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            TestBufferNulls();
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            TestBufferRemove(index);
            index++;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            index = TestBufferNulls();
        }
    }
    public void TestBufferSpawn()
    {
        BuffersControl.Instance.SpawnEffect(Elements.Fire,SpellTypes.Projectile, points[0].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Fire,SpellTypes.Projectile, points[1].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Fire,SpellTypes.Projectile, points[2].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Fire,SpellTypes.Projectile, points[4].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Fire,SpellTypes.Projectile, points[5].transform, 1);
    }
    public int TestBufferNulls()
    {
        return BuffersControl.Instance.SpawnEffect(Elements.Lightning,SpellTypes.Projectile, points[1].transform, 1);
    }
    public void TestBufferRemove(int index)
    {
        BuffersControl.Instance.UnspawnEffect(Elements.Fire, SpellTypes.Projectile, index);
    }
    public void TesteElement(Elements ele, List<GameObject> points)
    {
        
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Projectile, points[0].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Projectile, points[1].transform, 2);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Explosion, points[2].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Explosion, points[3].transform, 2);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Area, points[4].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Area, points[5].transform, 2);
        
    }
    public void TesteType(SpellTypes type, List<GameObject> points)
    {

        BuffersControl.Instance.SpawnEffect(Elements.Fire,type, points[0].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Radiance,type, points[1].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Darkness,type, points[2].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Ice,type, points[3].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Earth,type, points[4].transform, 1);
        BuffersControl.Instance.SpawnEffect(Elements.Poison,type, points[5].transform, 1);
        
    }
}
