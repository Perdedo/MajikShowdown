using System.Collections.Generic;
using UnityEngine;

public class EnemyOld : Character
{
    [Header("Targeting Options")]
    public Player Target;
    public float TargetStoppingDistance;

    [Header("Crowding Options")]
    public int EnemyPriority = 0;
    public float NeighborRadius = 1;
    public float SeparationStrength = 1;
    public float AllignmentStrength = 1;
    //public float ColisionPushStrength = 1;
    Vector3 separationForce;
    Vector3 targetVector;
    // Update is called once per frame
    void Update()
    {
        Target = GetClosestPlayer();
        if (Target != null)
        {
            targetVector = (Target.transform.position - transform.position);
            targetVector.y = 0;
            PathToTarget();
        }
        else
        {
            targetVector = Vector3.zero;
        }
        //Debug.Log(acceleration.magnitude);
        
    }
    public void PathToTarget()
    {
        separationForce = Vector3.zero;
        List<EnemyOld> neighbors = GeNeighbors();
        if (targetVector.magnitude > TargetStoppingDistance)
        {
            if (neighbors.Count > 0)
            {
                CalculateSeparationForce(neighbors);
                CalculateAllignment(neighbors);
            }
            Vector3 vel = Vector3.ClampMagnitude((targetVector + separationForce),1) * speed;
            /*if(vel.magnitude < speed)
            {
                vel = new Vector3(0, rb.linearVelocity.y, 0);
            }*/
            Move(vel);
        }
        else if (targetVector.magnitude < TargetStoppingDistance - TargetStoppingDistance / 10)
        {
            Move(-targetVector.normalized*speed);
        }
        else
        {
            /*if (neighbors.Count > 0)
            {
                CalculateSeparationForce(neighbors);
                SetVelocity(separationForce.normalized*speed);
            }
            else
            {
                SetVelocity(new Vector3(0, rb.linearVelocity.y, 0));
            }*/
            SetVelocity(new Vector3(0, rb.linearVelocity.y, 0));

        }
    }
    public List<EnemyOld> GeNeighbors()
    {
        var temp = Physics.OverlapSphere(transform.position, NeighborRadius, LayerMask.GetMask("Enemy"));
        List<EnemyOld> neighbors = new List<EnemyOld>();
        foreach (Collider col in temp)
        {
            EnemyOld e = col.GetComponent<EnemyOld>();
            if (e != null && e != this)
            {
                neighbors.Add(e);
            }
        }
        return neighbors;
    }
    public void CalculateSeparationForce(List<EnemyOld> Neighbors)
    {
        foreach (EnemyOld e in Neighbors)
        {
            if (e.EnemyPriority >= EnemyPriority)
            {
                Vector3 dir = transform.position - e.transform.position;
                if (dir.magnitude > 0)
                {
                    separationForce += (dir.normalized / dir.magnitude)*SeparationStrength;
                }
            }
            
        }
    }
    public void CalculateAllignment(List<EnemyOld> Neighbors)
    {
        foreach (EnemyOld e in Neighbors)
        {
            if (e.EnemyPriority >= EnemyPriority && e.Target == Target)
            {
                separationForce += e.transform.forward*AllignmentStrength;
            }
        }
    }
    public Player GetClosestPlayer()
    {
        Player closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (Player p in GameManager.Instance.Players)
        {
            float distance = Vector3.Distance(transform.position, p.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = p;
            }
        }
        return closest;
    }
    protected override void HandleRotation()
    {
        RotateTowards(targetVector.normalized);
    }
    /*private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 pushDir = contact.normal;
                pushDir.y = 0;
                float pushStrenght = Vector3.Dot(localVelocity, pushDir);//*rb.linearVelocity.magnitude;
                if(pushStrenght > 0)
                {
                    Vector3 push = -pushDir * pushStrenght*ColisionPushStrength;
                    AddVelocity(push);
                    //AddAcceleration(push);
                }
            }
        }
    }
    /*private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Vector3 normal = (other.transform.position - transform.position).normalized;
            Vector3 pushDir = normal;
            pushDir.y = 0;
            float pushStrenght = Vector3.Dot(localVelocity, pushDir);//*rb.linearVelocity.magnitude;
            if (pushStrenght > 0)
            {
                Vector3 push = -pushDir * pushStrenght * ColisionPushStrength;
                AddVelocity(push);
                //AddAcceleration(push);
            }
            Debug.Log("aaaa");
        }
    }*/
}
