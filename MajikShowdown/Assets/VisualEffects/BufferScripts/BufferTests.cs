using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;

public class BufferTests : MonoBehaviour
{
    [SerializeField]private List<GameObject> points = new List<GameObject>();
    public int index = 2;
    int i, i2;
    void Start()
    {
        Debug.Log(testefUNC(5.456f));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            TesteType(SpellTypes.Explosion,points);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            /*if(index == 1)
            {
                TestBufferRemove(0, Elements.Radiance, SpellTypes.Area);
                index++;
            }*/ 
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
    public void TestBufferRemove(int index, Elements ele, SpellTypes type)
    {
        BuffersControl.Instance.UnspawnEffect(ele, type, index);
    }
    public void TesteElement(Elements ele, List<GameObject> points)
    {
        
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Projectile, points[0].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Projectile, points[1].transform, 2);
        i2 =BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Explosion, points[2].transform, 1.5f, 1.5f);
        i = BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Explosion, points[3].transform, 2.5f, 5.5f);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Area, points[4].transform, 1);
        BuffersControl.Instance.SpawnEffect(ele,SpellTypes.Area, points[5].transform, 2);
        
    }
    public void TesteType(SpellTypes type, List<GameObject> points)
    {
        if(type != SpellTypes.Explosion)
        {
            BuffersControl.Instance.SpawnEffect(Elements.Fire,type, points[0].transform, 1);
            BuffersControl.Instance.SpawnEffect(Elements.Radiance,type, points[1].transform, 1);
            BuffersControl.Instance.SpawnEffect(Elements.Darkness,type, points[2].transform, 1);
            BuffersControl.Instance.SpawnEffect(Elements.Ice,type, points[3].transform, 1);
            BuffersControl.Instance.SpawnEffect(Elements.Earth,type, points[4].transform, 1);
            BuffersControl.Instance.SpawnEffect(Elements.Poison,type, points[5].transform, 1);
        }
        else
        {
            BuffersControl.Instance.SpawnEffect(Elements.Fire,type, points[0].transform, 1, 1.25f);
            BuffersControl.Instance.SpawnEffect(Elements.Radiance,type, points[1].transform, 1, 1.5f);
            BuffersControl.Instance.SpawnEffect(Elements.Darkness,type, points[2].transform, 1, 2f);
            BuffersControl.Instance.SpawnEffect(Elements.Ice,type, points[3].transform, 1, 2.25f);
            BuffersControl.Instance.SpawnEffect(Elements.Earth,type, points[4].transform, 1, 2.5f);
            BuffersControl.Instance.SpawnEffect(Elements.Poison,type, points[5].transform, 1, 3f);
        }
        
        
    }
    Vector2 testefUNC(float dec)
    {
        float x = Mathf.Floor(dec * 10);
        float y =  10 * ((dec * 10) - x );
        Vector2 vec = new Vector2(x/10,y);
        return vec;
    }
}
