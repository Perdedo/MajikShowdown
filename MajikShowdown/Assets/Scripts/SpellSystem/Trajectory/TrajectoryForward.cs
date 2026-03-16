using UnityEngine;

[CreateAssetMenu(fileName = "Forward Node", menuName = "Spell Nodes/Trajectory Nodes/Forward")]
public class TrajectoryForward : SpellTrajectory
{
    public override Vector3 GetTrajectory()
    {
        return Vector3.forward;
    }
}
