using UnityEngine;

[CreateAssetMenu(fileName = "Cast Point Node", menuName = "Spell Nodes/Cast Point Node")]
public class SpellCastPoint : SpellNode
{
    public enum CastPoints { AboveCaster, AimPoint, AboveAimPoint }
    public CastPoints castPoint;
    public float Offset = 10f;

    public Vector3 GetCastPoint(Transform caster, Vector3 aimPoint)
    {
        switch (castPoint)
        {
            case CastPoints.AboveCaster:
                return caster.position + Vector3.up * Offset;
            case CastPoints.AimPoint:
                return aimPoint;
            case CastPoints.AboveAimPoint:
                return aimPoint + Vector3.up * Offset;
            default:
                return caster.position;
        }
    }
}
