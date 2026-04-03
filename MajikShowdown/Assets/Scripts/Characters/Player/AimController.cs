using UnityEngine;

public class AimController : MonoBehaviour
{
    public Transform Follow;
    public Vector3 FollowOffset;
    public float MaxRayDistance = float.MaxValue;
    public LayerMask HitLayer;
    public bool ShowHit = true;
    RaycastHit Hit;
    Vector2 screenCenter;
    void Awake()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out Hit, MaxRayDistance, HitLayer))
        {
            transform.LookAt(Hit.point);
        }
        else
        {
            transform.LookAt(transform.position + ray.direction);
        }
        transform.position = Follow.transform.position + FollowOffset;
    }
    void OnDrawGizmos()
    {
        if (ShowHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Hit.point, 0.3f);
        }
        
    }
}
