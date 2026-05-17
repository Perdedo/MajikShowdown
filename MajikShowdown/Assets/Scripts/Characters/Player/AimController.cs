using System;
using UnityEngine;

public class AimController : MonoBehaviour
{
    public Transform Follow;
    public Vector3 FollowOffset;
    public float MaxRayDistance = float.MaxValue;
    public LayerMask HitLayer;
    public bool ShowHit = true;
    RaycastHit Hit;
    [NonSerialized]public Vector3 AimPoint;
    Vector2 screenCenter;
    int previousScreenWidth, previousScreenHeight;
    void Awake()
    {
        UpdateScreenCenter();

    }
    public void UpdateScreenCenter()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        previousScreenWidth = Screen.width;
        previousScreenHeight = Screen.height;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width != previousScreenWidth || Screen.height != previousScreenHeight)
        {
            UpdateScreenCenter();
        }
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out Hit, MaxRayDistance, HitLayer))
        {
            //transform.LookAt(Hit.point);
            AimPoint = Hit.point;
        }
        else
        {
            //transform.LookAt(transform.position + ray.direction);
            AimPoint = transform.position + ray.direction * MaxRayDistance;
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
