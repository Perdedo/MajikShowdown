using UnityEngine;

[CreateAssetMenu(fileName = "Cast Point Node", menuName = "Spell Nodes/Cast Point Node")]
public class SpellCastPoint : SpellNode
{
    public enum CastPoints { AboveCaster, AimPoint, AboveAimPoint }
    public CastPoints castPoint;

    public Vector3 GetCastPoint(Transform caster, Vector3 aimPoint)
    {
        switch (castPoint)
        {
            case CastPoints.AboveCaster:
                return caster.position + Vector3.up * 10f;
            case CastPoints.AimPoint:
                return aimPoint;
            case CastPoints.AboveAimPoint:
                return aimPoint + Vector3.up * 10f;
            default:
                return caster.position;
        }
    }
    public override void SetupNodeVisual()
    {
        color = HexToColor("E84A4A");

        ConectionPorts = new NodeConection.Conections[]
        {
        NodeConection.Conections.None,
        NodeConection.Conections.Penta,
        NodeConection.Conections.None,
        NodeConection.Conections.None,
        NodeConection.Conections.None,
        NodeConection.Conections.None
        };
    }
}
