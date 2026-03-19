using UnityEngine;
using UnityEngine.Events;

public class SpellCollider : MonoBehaviour
{
    public StaticRB rb;
    public StatTypes stats;
    public SubSpell subSpell;

    public UnityEvent OnCast, OnHit, OnDeath;
    private void Start()
    {
        //projectileConfig.CalculateFinalStats();
        stats = subSpell.Type.FinalStats;
        transform.localScale = Vector3.one * stats.Size;
        Invoke("Die", stats.Duration);
        OnCast.Invoke();
    }
    void Update()
    {
        rb.Velocity = ToLookDirection(subSpell.Type.GetVelocity());
    }
    public Vector3 ToLookDirection(Vector3 rawDir)
    {
        return transform.forward*rawDir.z + transform.up*rawDir.y + transform.right*rawDir.x;
    }
    public void Die()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }
}
