using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public abstract void Interact(Player player);
    public virtual void OnEnable()
    {
        GameManager.Instance.AddInteractable(this);
    }
    public virtual void OnDisable()
    {
        GameManager.Instance.RemoveInteractable(this);
    }
    public virtual void CheckForPlayer()
    {
        foreach (Player p in GameManager.Instance.Players)
        {
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist <= GameManager.Instance.interactionRadius)
            {
                if (p.currentInteraction == null || dist < Vector3.Distance(p.transform.position, p.currentInteraction.transform.position))
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
