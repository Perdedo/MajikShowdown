using UnityEngine;

public abstract class SpellTrajectory : SpellNode
{
    public abstract Vector3 GetTrajectory();
}
public class TrajectoryFoward : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
public class TrajectoryLobbed : SpellTrajectory
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
