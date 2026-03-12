using UnityEngine;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public SpellType projectileConfig;
    public float Size = 1;
    void Start()
    {
        /*projectileConfig = new TypeProjectile();
        projectileConfig.BaseStats.Speed = 1;
        projectileConfig.BaseStats.Size = 1;
        TrajectoryFoward test = new TrajectoryFoward();
        test.BaseStats.Size = 4;
        projectileConfig.TryConectNode(test,0);*/
        projectileConfig.CalculateFinalStats();
        Size = projectileConfig.FinalStats.Size;
        transform.localScale = Vector3.one * Size;
    }
    void Update()
    {
        rb.Velocity = projectileConfig.GetVelocity();
    }
}
