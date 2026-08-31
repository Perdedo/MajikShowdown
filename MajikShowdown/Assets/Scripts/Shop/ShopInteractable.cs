using UnityEngine;

public class ShopInteractable : InteractableObject
{
    [SerializeField] private Collider interactionArea;

    public override void Interact(Player player)
    {
        if (player == null) return;
        PlayerUI playerUI = player.transform.parent ?.GetComponentInChildren<PlayerUI>(true);
        if (playerUI == null) return;
        playerUI.OpenShopPanel();
    }

    public override void CheckForPlayer()
    {
        if (interactionArea == null) return;
        foreach (Player player in GameManager.Instance.Players)
        {
            Vector3 closestPoint = interactionArea.ClosestPoint(player.transform.position);
            float distance = Vector3.Distance(player.transform.position, closestPoint);
            if (distance <= GameManager.Instance.interactionRadius)
            {
                if (player.currentInteraction == null)
                {
                    player.currentInteraction = this;
                }
            }
            else if (player.currentInteraction == this)
            {
                player.currentInteraction = null;
            }
        }
    }
}