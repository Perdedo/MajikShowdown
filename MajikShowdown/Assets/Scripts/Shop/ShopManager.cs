using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop")]
    [SerializeField] private ShopSlot[] slots;

    [Header("Player Shop")]
    [SerializeField] private PlayerShop playerShop;

    private void Start()
    {
        if (playerShop == null &&
            GameManager.Instance != null &&
            GameManager.Instance.uiController != null &&
            GameManager.Instance.uiController.playerUI != null)
        {
            Player player = GameManager.Instance.uiController.playerUI.myPlayer;

            if (player != null)
            {
                playerShop = player.GetComponent<PlayerShop>();
            }
        }
    }

    public void RefreshShop()
    {
        if (playerShop == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            ShopOffer offer = playerShop.GetOffer(i);

            if (offer == null)
            {
                slots[i].Clear();
                continue;
            }

            slots[i].Setup(
                offer.node,
                offer.price,
                this
            );

            if (offer.purchased)
            {
                slots[i].SetPurchased();
            }
        }
    }

    public void TryBuy(ShopSlot slot)
    {
        if (playerShop == null) return;
        if (slot == null) return;

        int slotIndex = System.Array.IndexOf(slots, slot);

        if (slotIndex < 0) return;

        playerShop.TryBuy(slotIndex);
    }

    public void RerollShop()
    {
        if (playerShop == null) return;

        playerShop.TryReroll();
    }
}