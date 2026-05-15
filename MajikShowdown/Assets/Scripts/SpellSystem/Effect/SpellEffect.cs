using UnityEngine;

public abstract class SpellEffect : SpellNode
{
    public virtual bool Repeatable {protected set; get; } = false;
    public abstract void ApplyEffect(CharacterDamageHandler target);

    public override void SetupNodeVisual()
    {
        color = HexToColor("883ED6");

        ConectionPorts = new NodeConection.Conections[]
        {
            NodeConection.Conections.None,
            NodeConection.Conections.None,
            NodeConection.Conections.Triangle,
            NodeConection.Conections.None,
            NodeConection.Conections.None,
            NodeConection.Conections.Triangle
        };
    }
}

