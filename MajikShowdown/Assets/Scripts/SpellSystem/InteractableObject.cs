using Mirror;
using UnityEngine;

public abstract class InteractableObject : NetworkBehaviour
{
    [Header("Interaction")]
    [SerializeField] private string interactionMessage = "[F] Interact";
    [SerializeField] private WorldInteractionIndicator interactionIndicator;

    public string InteractionMessage => interactionMessage;

    public abstract void Interact(Player player);

    public virtual void OnEnable()
    {
        GameManager.Instance.AddInteractable(this);
    }

    public virtual void OnDisable()
    {
        GameManager.Instance.RemoveInteractable(this);

        if (interactionIndicator != null)
        {
            interactionIndicator.Hide();
        }
    }

    public void ShowInteractionIndicator(Player player)
    {
        if (interactionIndicator == null) return;

        interactionIndicator.Show(interactionMessage, player);
    }

    public void HideInteractionIndicator()
    {
        if (interactionIndicator == null) return;

        interactionIndicator.Hide();
    }

    public virtual void CheckForPlayer()
    {
        foreach (Player p in GameManager.Instance.Players)
        {
            float dist = Vector3.Distance(
                p.transform.position,
                transform.position
            );

            if (dist <= GameManager.Instance.interactionRadius)
            {
                if (p.currentInteraction == null ||
                    dist < Vector3.Distance(
                        p.transform.position,
                        p.currentInteraction.transform.position
                    ))
                {
                    p.currentInteraction = this;
                }
            }
            else if (p.currentInteraction == this)
            {
                p.currentInteraction = null;
            }
        }
    }
}