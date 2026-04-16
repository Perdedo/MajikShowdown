using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Trajectory Node", menuName = "Spell Nodes/Trajectory Node")]
public class SpellTrajectory : SpellNode
{
    public enum TragectoryType { Forward, Lobbed, Orbital, ZigZag, FollowEnemy, FollowCaster, FollowAlly };
    public TragectoryType tragectory;
    public Vector3 GetTrajectory(float lifetime)
    {
        Vector3 dir = Vector3.zero;
        switch (tragectory)
        {
            case TragectoryType.Forward:
                dir = Vector3.forward;
                break;
            case TragectoryType.ZigZag:
                dir = Vector3.right * Mathf.Sin(lifetime * 10 + math.PI / 2);
                break;

            /*case TragectoryType.Orbital:
                float x = Mathf.Cos(lifetime*5) * 0.1f;
                float z = Mathf.Sin(lifetime*5) * 0.1f;
                dir = new Vector3(x, 0, z);
                break;*/

            default:
                dir = Vector3.zero;
                break;
        }
        if (ConectedNodes[0] is SpellTrajectory t)
        {
            return (dir + t.GetTrajectory(lifetime)).normalized;
        }
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
