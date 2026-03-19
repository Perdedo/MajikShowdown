using UnityEngine;

[CreateAssetMenu(fileName = "Trajectory Node", menuName = "Spell Nodes/Trajectory Node")]
public class SpellTrajectory : SpellNode
{
    public enum TragectoryType { Forward, Lobbed, Orbital, ZigZag, FollowEnemy, FollowCaster, FollowAlly };
    public TragectoryType tragectory;
    public Vector3 GetTrajectory()
    {
        switch (tragectory)
        {
            case TragectoryType.Forward:
                return Vector3.forward;
            default: return Vector3.zero;
        }
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
