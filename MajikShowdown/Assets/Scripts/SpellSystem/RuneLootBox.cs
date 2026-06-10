using UnityEngine;

public class RuneLootBox : InteractableObject
{
    public RuneLootPool lootPool;
    public SpellNode GetLoot()
    {
        if (lootPool == null)
        {
            Debug.LogError("No loot pool assigned to loot box");
            return null;
        }
        return lootPool.GetLoot();
    }

    public override void Interact(Player player)
    {
        player.caster.AddRune(GetLoot());
        LootSpawner.Instance.DespawnLootBox(this);
    }
}
