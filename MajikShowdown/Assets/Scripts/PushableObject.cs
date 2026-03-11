using UnityEngine;

public class PushableObject : FloatingRigidbody
{
    [SerializeField]Player player;
    Vector3 contactPoint;
    private void Start()
    {
        rb.constraints = ~RigidbodyConstraints.FreezePositionY; ;
    }
    protected override void FixedUpdate()
    {
        Float();
        UpdateVelocity();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<Player>() != null && collision.collider.GetComponent<Player>().pushing == null)
        {
            StartPush(collision.collider.GetComponent<Player>());
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.GetComponent<Player>() != null && collision.collider.GetComponent<Player>().pushing == this)
        {
            StopPush();
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.GetComponent<Player>() != null && (collision.collider.GetComponent<Player>().pushing == this || collision.collider.GetComponent<Player>().pushing == null))
        {
            CheckAngles(collision.collider.GetComponent<Player>());
        }
    }
    void CheckAngles(Player p)
    {
        if (Vector3.Angle(p.localVelocity, (transform.position - p.transform.position)) <= 40)
        {
            if(player == null)
            {
                StartPush(p);
            }
            SetVelocity(Vector3Utility.highestAxis(new Vector3(player.localVelocity.x, 0, player.localVelocity.z)));
        }
        else if(player != null)
        {
            StopPush();
            SetVelocity(Vector3.zero);
        }
    }
    void StartPush(Player p)
    {
        player = p;
        player.pushing = this;
        player.StartedPushing.Invoke();
        rb.constraints = ~RigidbodyConstraints.FreezePosition;
    }
    void StopPush()
    {
        player.pushing = null;
        player.StoppedPushing.Invoke();
        player = null;
        rb.constraints = ~RigidbodyConstraints.FreezePositionY;
    }
}
