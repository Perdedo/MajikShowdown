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
        int qualityInd = -1;
        int typeInd = -1;
        int listInd = -1;
        RuneQualityGroup aux = null;
        switch(loot.quality)
        {
            case SpellNode.Quality.Rusty:
                aux = lootPool.Rusty;
                qualityInd = 0;
                break;
            case SpellNode.Quality.Forged:
                aux = lootPool.Forged;
                qualityInd = 1;
                break;
            case SpellNode.Quality.FactoryNew:
                aux = lootPool.FactoryNew;
                qualityInd = 2;
                break;
            /*case SpellNode.Quality.Epic:
                aux = lootPool.Epic;
                rarityInd = 3;
                break;
            case SpellNode.Quality.Legendary:
                aux = lootPool.Legendary;
                rarityInd = 4;
                break;*/
        }
        if(loot is SpellCore)
        {
            typeInd = 0;
            listInd = aux.Core.IndexOf(loot as SpellCore);
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

        RPCGetLoot(qualityInd, typeInd, listInd);
    }

    [ClientRpc]
    public void RPCGetLoot(int rarityInd, int typeInd, int listInd)
    {
        if(isServer)
        {
            return;
        }
        lootPool = LootSpawner.Instance.lootPools[lootPoolInd];
        RuneQualityGroup aux = null;
        switch(rarityInd)
        {
            case 0:
                aux = lootPool.Rusty;
                break;
            case 1:
                aux = lootPool.Forged;
                break;
            case 2:
                aux = lootPool.FactoryNew;
                break;
            /*case 3:
                aux = lootPool.Epic;
                break;
            case 4:
                aux = lootPool.Legendary;
                break;*/
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
        player.caster.AddRune(loot);

        if (player.isLocalPlayer)
        {
            GameManager.Instance.uiController.playerUI.ShowRunePickup(loot, transform.position);
        }

        if (!isServer)
        {
            CMDInteract(GameManager.Instance.Players.IndexOf(player));
        }
        else
        {
            LootSpawner.Instance.DespawnLootBox(this);
        }
    }

    [Command(requiresAuthority = false)]
    public void CMDInteract(int playerInd)
    {
        GameManager.Instance.Players[playerInd].caster.AddRune(loot);
        LootSpawner.Instance.DespawnLootBox(this);
    }
}
