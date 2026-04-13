using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropArea : MonoBehaviour, IDropHandler
{
    public NodeInventory inventory;

    public void OnDrop(PointerEventData eventData)
    {
        var node = eventData.pointerDrag?.GetComponent<DraggableNode>();
        if (node == null) return;
        node.RegisterDrop(inventory);
    }
}