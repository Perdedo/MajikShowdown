using Mirror;
using UnityEngine;

public class PlayerShop : NetworkBehaviour
{
    [Header("Shop")]
    [SerializeField] private RuneLootPool lootPool;
    [SerializeField] private int offerCount = 3;

    [Header("Common Price")]
    [SerializeField] private int commonMinPrice = 350;
    [SerializeField] private int commonMaxPrice = 500;

    [Header("Uncommon Price")]
    [SerializeField] private int uncommonMinPrice = 900;
    [SerializeField] private int uncommonMaxPrice = 1500;

    [Header("Rare Price")]
    [SerializeField] private int rareMinPrice = 2000;
    [SerializeField] private int rareMaxPrice = 3500;

    [Header("Epic Price")]
    [SerializeField] private int epicMinPrice = 5000;
    [SerializeField] private int epicMaxPrice = 8000;

    [Header("Legendary Price")]
    [SerializeField] private int legendaryMinPrice = 10000;
    [SerializeField] private int legendaryMaxPrice = 15000;

    [Header("Price Settings")]
    [SerializeField] private int priceStep = 25;

    [Header("Reroll")]
    [SerializeField] private int rerollPrice = 500;

    private Player player;
    private ShopOffer[] offers;

    public int RerollPrice => rerollPrice;

    private void Awake()
    {
        player = GetComponent<Player>();
        offers = new ShopOffer[offerCount];
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        GenerateOffers();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        CMDRequestShop();
    }

    public ShopOffer GetOffer(int index)
    {
        if (index < 0 || index >= offers.Length)
        {
            return null;
        }

        return offers[index];
    }

    public void TryBuy(int slotIndex)
    {
        if (!isOwned) return;

        CMDBuy(slotIndex);
    }

    public void TryReroll()
    {
        if (!isOwned) return;

        CMDReroll();
    }

    [Command]
    private void CMDRequestShop()
    {
        SendShopToOwner();
    }

    [Command]
    private void CMDBuy(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= offers.Length)
        {
            return;
        }

        ShopOffer offer = offers[slotIndex];

        if (offer == null) return;
        if (offer.node == null) return;
        if (offer.purchased) return;

        if (!player.SpendMoney(offer.price))
        {
            return;
        }
        player.caster.AddRune(offer.node);
        offer.purchased = true;
        TargetGiveRune(connectionToClient, GetNodeRarityIndex(offer.node), GetNodeTypeIndex(offer.node), GetNodeListIndex(offer.node));
        TargetPurchaseCompleted(connectionToClient, slotIndex);
    }

    [Command]
    private void CMDReroll()
    {
        if (!player.SpendMoney(rerollPrice))
        {
            return;
        }

        GenerateOffers();
        SendShopToOwner();
    }

    [Server]
    private void GenerateOffers()
    {
        if (lootPool == null)
        {
            Debug.LogError("No RuneLootPool assigned to PlayerShop.");
            return;
        }

        for (int i = 0; i < offers.Length; i++)
        {
            SpellNode node = lootPool.GetLoot();

            if (node == null)
            {
                offers[i] = null;
                continue;
            }

            offers[i] = new ShopOffer
            {
                node = node,
                price = GetRunePrice(node),
                purchased = false
            };
        }
    }

    [Server]
    private void SendShopToOwner()
    {
        for (int i = 0; i < offers.Length; i++)
        {
            ShopOffer offer = offers[i];

            if (offer == null || offer.node == null)
            {
                TargetClearOffer(connectionToClient, i);
                continue;
            }

            TargetSetOffer(
                connectionToClient,
                i,
                GetNodeRarityIndex(offer.node),
                GetNodeTypeIndex(offer.node),
                GetNodeListIndex(offer.node),
                offer.price,
                offer.purchased
            );
        }

        TargetRefreshShop(connectionToClient);
    }

    [TargetRpc]
    private void TargetSetOffer(
        NetworkConnection target,
        int slotIndex,
        int rarityIndex,
        int typeIndex,
        int listIndex,
        int price,
        bool purchased)
    {
        SpellNode node = GetNodeFromIndexes(
            rarityIndex,
            typeIndex,
            listIndex
        );

        offers[slotIndex] = new ShopOffer
        {
            node = node,
            price = price,
            purchased = purchased
        };
    }

    [TargetRpc]
    private void TargetClearOffer(NetworkConnection target, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= offers.Length)
        {
            return;
        }

        offers[slotIndex] = null;
    }

    [TargetRpc]
    private void TargetPurchaseCompleted(NetworkConnection target, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= offers.Length)
        {
            return;
        }

        if (offers[slotIndex] != null)
        {
            offers[slotIndex].purchased = true;
        }

        RefreshLocalShop();
    }

    [TargetRpc]
    private void TargetRefreshShop(NetworkConnection target)
    {
        RefreshLocalShop();
    }

    private void RefreshLocalShop()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.uiController == null) return;
        if (GameManager.Instance.uiController.playerUI == null) return;

        ShopManager shopManager =
            GameManager.Instance.uiController.playerUI.GetComponentInChildren<ShopManager>(true);

        if (shopManager != null)
        {
            shopManager.RefreshShop();
        }
    }

    private int GetRunePrice(SpellNode node)
    {
        switch (node.rarity)
        {
            case SpellNode.Rarity.Common:
                return GetRandomPrice(commonMinPrice, commonMaxPrice);

            case SpellNode.Rarity.Uncommon:
                return GetRandomPrice(uncommonMinPrice, uncommonMaxPrice);

            case SpellNode.Rarity.Rare:
                return GetRandomPrice(rareMinPrice, rareMaxPrice);

            case SpellNode.Rarity.Epic:
                return GetRandomPrice(epicMinPrice, epicMaxPrice);

            case SpellNode.Rarity.Legendary:
                return GetRandomPrice(legendaryMinPrice, legendaryMaxPrice);

            default:
                return commonMinPrice;
        }
    }

    private int GetRandomPrice(int minPrice, int maxPrice)
    {
        if (priceStep <= 0)
        {
            return Random.Range(minPrice, maxPrice + 1);
        }

        int minStep = Mathf.CeilToInt((float)minPrice / priceStep);
        int maxStep = Mathf.FloorToInt((float)maxPrice / priceStep);

        if (maxStep < minStep)
        {
            return minPrice;
        }

        return Random.Range(minStep, maxStep + 1) * priceStep;
    }

    private int GetNodeRarityIndex(SpellNode node)
    {
        return (int)node.rarity;
    }

    private int GetNodeTypeIndex(SpellNode node)
    {
        if (node is SpellType) return 0;
        if (node is SpellTrajectory) return 1;
        if (node is SpellEffect) return 2;
        if (node is SpellStat) return 3;
        if (node is SpellTrigger) return 4;
        if (node is SpellCastPoint) return 5;

        return -1;
    }

    private int GetNodeListIndex(SpellNode node)
    {
        RuneRaretyGroup group = GetRarityGroup(node.rarity);

        if (group == null)
        {
            return -1;
        }

        if (node is SpellType)
        {
            return group.Core.IndexOf(node as SpellType);
        }

        if (node is SpellTrajectory)
        {
            return group.Trajectory.IndexOf(node as SpellTrajectory);
        }

        if (node is SpellEffect)
        {
            return group.Effect.IndexOf(node as SpellEffect);
        }

        if (node is SpellStat)
        {
            return group.Stat.IndexOf(node as SpellStat);
        }

        if (node is SpellTrigger)
        {
            return group.Trigger.IndexOf(node as SpellTrigger);
        }

        if (node is SpellCastPoint)
        {
            return group.CastPoint.IndexOf(node as SpellCastPoint);
        }

        return -1;
    }

    private SpellNode GetNodeFromIndexes(int rarityIndex, int typeIndex, int listIndex)
    {
        RuneRaretyGroup group = GetRarityGroup((SpellNode.Rarity)rarityIndex);

        if (group == null)
        {
            return null;
        }

        switch (typeIndex)
        {
            case 0:
                if (listIndex >= 0 && listIndex < group.Core.Count)
                    return group.Core[listIndex];
                break;

            case 1:
                if (listIndex >= 0 && listIndex < group.Trajectory.Count)
                    return group.Trajectory[listIndex];
                break;

            case 2:
                if (listIndex >= 0 && listIndex < group.Effect.Count)
                    return group.Effect[listIndex];
                break;

            case 3:
                if (listIndex >= 0 && listIndex < group.Stat.Count)
                    return group.Stat[listIndex];
                break;

            case 4:
                if (listIndex >= 0 && listIndex < group.Trigger.Count)
                    return group.Trigger[listIndex];
                break;

            case 5:
                if (listIndex >= 0 && listIndex < group.CastPoint.Count)
                    return group.CastPoint[listIndex];
                break;
        }

        return null;
    }

    private RuneRaretyGroup GetRarityGroup(SpellNode.Rarity rarity)
    {
        switch (rarity)
        {
            case SpellNode.Rarity.Common:
                return lootPool.Common;

            case SpellNode.Rarity.Uncommon:
                return lootPool.Uncommon;

            case SpellNode.Rarity.Rare:
                return lootPool.Rare;

            case SpellNode.Rarity.Epic:
                return lootPool.Epic;

            case SpellNode.Rarity.Legendary:
                return lootPool.Legendary;

            default:
                return null;
        }
    }

    [TargetRpc]
    private void TargetGiveRune(NetworkConnection target, int rarityIndex, int typeIndex, int listIndex)
    {
        if (isServer) return;

        SpellNode node = GetNodeFromIndexes(rarityIndex, typeIndex, listIndex);

        if (node == null) return;
        if (player == null) return;
        if (player.caster == null) return;

        player.caster.AddRune(node);
    }
}

public class ShopOffer
{
    public SpellNode node;
    public int price;
    public bool purchased;
}