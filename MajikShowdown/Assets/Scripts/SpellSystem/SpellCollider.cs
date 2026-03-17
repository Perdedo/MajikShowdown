using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public StatTypes stats;
    public SpellType projectileConfig;

    public UnityEvent OnCast, OnHit, OnDeath;
    private void Start()
    {
        //projectileConfig.CalculateFinalStats();
        stats = projectileConfig.FinalStats;
        transform.localScale = Vector3.one * stats.Size;
        Invoke("Die", stats.Duration);
        OnCast.Invoke();
    }
    void Update()
    {
        rb.Velocity = projectileConfig.GetVelocity();
    }
    public void Die()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }
}
