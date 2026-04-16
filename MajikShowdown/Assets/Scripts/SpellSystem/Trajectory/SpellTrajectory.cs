using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Trajectory Node", menuName = "Spell Nodes/Trajectory Node")]
public class SpellTrajectory : SpellNode
{
    public enum TrajectoryType { Forward, Lobbed, Orbital, ZigZag, FollowEnemy, FollowCaster, FollowAlly, Spiral, Boomerang };
    public TrajectoryType trajectoryType;
    public Vector3 GetTrajectory(SpellCollider collider)
    {
        Vector3 dir = Vector3.zero;
        switch (trajectoryType)
        {
            case TrajectoryType.Forward:
                dir = collider.transform.forward;
                break;
            case TrajectoryType.ZigZag:
                dir = collider.ToLookDirection(new Vector3(Mathf.Sin(collider.LifeTime * 10 + math.PI / 2),0,1));
                break;

            case TrajectoryType.Orbital:
                float x = Mathf.Cos(collider.LifeTime*5) * 0.1f;
                float z = Mathf.Sin(collider.LifeTime*5) * 0.1f;
                dir = new Vector3(x, 0, z);
                break;

            case TrajectoryType.Spiral:
            float X = Mathf.Sin(collider.LifeTime * 10 + math.PI / 2);
            float Y = Mathf.Sin(collider.LifeTime * 10);
            dir = collider.ToLookDirection(new Vector3(X,Y,1));
            break;

            case TrajectoryType.Boomerang:
            if(collider.LifeTime/OwnerSpell.primaryNode.FinalStats.Duration < 0.5f)
                {
                    dir = collider.transform.forward;
                }
                else
                {
                    Vector3 distance = OwnerSpell.Caster.CastingPoint.position - collider.transform.position;
                    float multiplier = Mathf.Max(distance.magnitude/(OwnerSpell.primaryNode.FinalStats.Speed*OwnerSpell.primaryNode.FinalStats.Duration/2), 1);
                    dir = distance.normalized *multiplier;
                    if(distance.magnitude < 0.1f)
                    {
                        collider.Die();
                    }
                }
            //dir = Vector3.forward * Mathf.Sin((lifetime/OwnerSpell.primaryNode.FinalStats.Duration)*Mathf.PI*2);
            break;

            default:
                dir = Vector3.zero;
                break;
        }
        /*if (ConectedNodes[0] is SpellTrajectory t)
        {
            return (dir + t.GetTrajectory(collider)).normalized;
        }*/
        return dir;
    }
}

/*public class TrajectoryLobbed : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
public class TrajectoryOrbital : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
public class TrajectoryZigZag : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
public class TrajectoryFollowEnemy : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
public class TrajectoryFollowCaster : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
public class TrajectoryFollowAlly : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
[CreateAssetMenu(fileName = "Forward Node", menuName = "Spell Nodes/Trajectory Nodes/Forward")]
public class TrajectoryForward : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}*/
