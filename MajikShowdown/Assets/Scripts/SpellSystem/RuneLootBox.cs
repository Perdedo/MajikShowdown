using Mirror;
using UnityEngine;

public class RuneLootBox : InteractableObject
{
    public RuneLootPool lootPool;
    [SyncVar]public int lootPoolInd;
    public SpellNode loot;
    public SpellNode GetLoot()
    {
        if (lootPool == null)
        {
            Debug.LogError("No loot pool assigned to loot box");
            return null;
        }
        return lootPool.GetLoot();
    }

    public void Initialize()
    {
        if(!isServer)
        {
            return;
        }
        lootPool = LootSpawner.Instance.lootPools[lootPoolInd];
        loot = GetLoot();
        int rarityInd = -1;
        int typeInd = -1;
        int listInd = -1;
        RuneRaretyGroup aux = null;
        switch(loot.rarity)
        {
            case SpellNode.Rarity.Common:
                aux = lootPool.Common;
                rarityInd = 0;
                break;
            case SpellNode.Rarity.Uncommon:
                aux = lootPool.Uncommon;
                rarityInd = 1;
                break;
            case SpellNode.Rarity.Rare:
                aux = lootPool.Rare;
                rarityInd = 2;
                break;
            case SpellNode.Rarity.Epic:
                aux = lootPool.Epic;
                rarityInd = 3;
                break;
            case SpellNode.Rarity.Legendary:
                aux = lootPool.Legendary;
                rarityInd = 4;
                break;
        }
        if(loot is SpellType)
        {
            typeInd = 0;
            listInd = aux.Core.IndexOf(loot as SpellType);
        }
        else if(loot is SpellTrajectory)
        {
            typeInd = 1;
            listInd = aux.Trajectory.IndexOf(loot as SpellTrajectory);
        }
        else if(loot is SpellEffect)
        {
            typeInd = 2;
            listInd = aux.Effect.IndexOf(loot as SpellEffect);
        }
        else if(loot is SpellStat)
        {
            typeInd = 3;
            listInd = aux.Stat.IndexOf(loot as SpellStat);
        }
        else if(loot is SpellTrigger)
        {
            typeInd = 4;
            listInd = aux.Trigger.IndexOf(loot as SpellTrigger);
        }
        else if(loot is SpellCastPoint)
        {
            typeInd = 5;
            listInd = aux.CastPoint.IndexOf(loot as SpellCastPoint);
        }

        RPCGetLoot(rarityInd, typeInd, listInd);
    }

    [ClientRpc]
    public void RPCGetLoot(int rarityInd, int typeInd, int listInd)
    {
        if(isServer)
        {
            return;
        }
        RuneRaretyGroup aux = null;
        switch(rarityInd)
        {
            case 0:
                aux = lootPool.Common;
                break;
            case 1:
                aux = lootPool.Uncommon;
                break;
            case 2:
                aux = lootPool.Rare;
                break;
            case 3:
                aux = lootPool.Epic;
                break;
            case 4:
                aux = lootPool.Legendary;
                break;
        }

        switch(typeInd)
        {
            case 0:
                loot = aux.Core[listInd];
                break;
            case 1:
                loot = aux.Trajectory[listInd];
                break;
            case 2:
                loot = aux.Effect[listInd];
                break;
            case 3:
                loot = aux.Stat[listInd];
                break;
            case 4:
                loot = aux.Trigger[listInd];
                break;
            case 5:
                loot = aux.CastPoint[listInd];
                break;
        }
    }


    public override void Interact(Player player)
    {
        //player.caster.AddRune(GetLoot());
        player.caster.AddRune(loot);
        if(!isServer)
        {
            CMDInteract(GameManager.Instance.Players.IndexOf(player));
        }
        LootSpawner.Instance.DespawnLootBox(this);
    }

    [Command]
    public void CMDInteract(int playerInd)
    {
        GameManager.Instance.Players[playerInd].caster.AddRune(loot);
    }
}
