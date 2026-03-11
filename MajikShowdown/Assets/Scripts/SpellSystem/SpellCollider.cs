using UnityEngine;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public SpellType projectileConfig;
    public float Size = 1;
    void Start()
    {
        projectileConfig = new TypeProjectile
        {
            Trajectory = new TrajectoryFoward()
        };
        projectileConfig.CalculateFinalStats();
        Size = projectileConfig.FinalStats.Size;
        transform.localScale = Vector3.one * Size;
    }
    void Update()
    {
        rb.Velocity = projectileConfig.GetVelocity();
    }
}
