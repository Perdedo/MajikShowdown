using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("Node Visual")]
    [SerializeField] private Image mainImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image symbolImage;

    [Header("UI")]
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text buyButtonText;
    [SerializeField] private Button buyButton;

    private SpellNode currentNode;
    private int currentPrice;
    private bool purchased;
    private ShopManager shopManager;

    public SpellNode CurrentNode => currentNode;
    public int CurrentPrice => currentPrice;
    public bool Purchased => purchased;

    public void Setup(SpellNode node, int price, ShopManager manager)
    {
        currentNode = node;
        currentPrice = price;
        purchased = false;
        shopManager = manager;

        if (currentNode == null)
        {
            Clear();
            return;
        }

        SetupNodeVisual();

        if (runeNameText != null)
        {
            runeNameText.text = currentNode.runeName;
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = currentPrice.ToString();
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
            buyButton.interactable = true;
        }
    }

    private void Buy()
    {
        if (shopManager == null) return;
        if (purchased) return;

        shopManager.TryBuy(this);
    }

    private void SetupNodeVisual()
    {
        if (currentNode.spellInfos == null) return;

        NodeVisualInfo info = currentNode.spellInfos.GetInfo(currentNode.GetCategory());

        if (mainImage != null)
        {
            mainImage.color = info.color;
        }

        if (borderImage != null)
        {
            borderImage.sprite = info.borderSprite;
        }

        if (symbolImage != null)
        {
            bool hasSymbol = currentNode.nodeSymbolSprite != null;

            symbolImage.gameObject.SetActive(hasSymbol);

            if (hasSymbol)
            {
                symbolImage.sprite = currentNode.nodeSymbolSprite;
                symbolImage.color = info.internSymbolColor;
            }
        }
    }

    public void SetPurchased()
    {
        purchased = true;

        if (buyButton != null)
        {
            buyButton.interactable = false;
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = "Sold";
        }
    }

    public void Clear()
    {
        currentNode = null;
        currentPrice = 0;
        purchased = false;

        if (mainImage != null)
        {
            mainImage.color = Color.white;
        }

        if (borderImage != null)
        {
            borderImage.sprite = null;
        }

        if (symbolImage != null)
        {
            symbolImage.sprite = null;
            symbolImage.gameObject.SetActive(false);
        }

        if (runeNameText != null)
        {
            runeNameText.text = "";
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = "";
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = false;
        }
    }
}