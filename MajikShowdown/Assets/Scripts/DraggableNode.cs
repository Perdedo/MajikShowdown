using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public CanvasGroup canvasGroup;
    [HideInInspector] public int acquisitionOrder;
    int savedListIndex = 0;
    private Vector2 savedPosition;
    private Transform savedParent;
    public IDropZone OriginZone { get; private set; }
    private IDropZone pendingDropZone;

    public bool isClone = false;
    public DraggableNode inventorySource;
    public DraggableNode inventoryClone;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetOriginZone(IDropZone zone) => OriginZone = zone;
    public void RegisterDrop(IDropZone dropZone) => pendingDropZone = dropZone;

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;
        pendingDropZone = null;
        canvas = GetComponentInParent<Canvas>();
        savedPosition = rectTransform.anchoredPosition;
        savedParent = transform.parent;
        var node = GetComponent<SpellNodeInterface>();
        node?.SelectOnly();
        var inventory = OriginZone as NodeInventory;
        var nodeInterface = GetComponent<SpellNodeInterface>();
        if (inventory != null && nodeInterface != null)
        {
            savedListIndex = inventory.GetNodeIndex(nodeInterface);
        }
        if (inventory != null && !isClone)
        {
            inventory.Freeze();
        }
        else
        {
            OriginZone?.Release(this);
        }
        transform.SetParent(canvas.transform, true);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        var inventory = OriginZone as NodeInventory;
        ResolveDrop(inventory);
        inventory?.Unfreeze();
    }

    private void ResolveDrop(NodeInventory inventory)
    {
        if (pendingDropZone != null && inventory != null && !isClone && pendingDropZone is HexGridNode)
        {
            GameObject cloneGO = Instantiate(gameObject, canvas.transform);
            DraggableNode clone = cloneGO.GetComponent<DraggableNode>();
            clone.isClone = true;
            clone.inventorySource = this;
            clone.canvas = canvas;
            clone.rectTransform = cloneGO.GetComponent<RectTransform>();
            clone.canvasGroup = cloneGO.GetComponent<CanvasGroup>();
            clone.canvasGroup.alpha = 1f;
            clone.canvasGroup.blocksRaycasts = true;
            var cloneInterface = cloneGO.GetComponent<SpellNodeInterface>();
            if (cloneInterface != null)
            {
                cloneInterface.SetUsed(true);
            }
            inventoryClone = clone;
            inventory.Receive(clone);
            inventory.InsertNodeAt(cloneInterface, savedListIndex);
            pendingDropZone.Receive(this);
        }
        else if (pendingDropZone != null)
        {
            if (isClone && pendingDropZone is NodeInventory)
            {
                var source = inventorySource;
                var spellNode = source?.GetComponent<SpellNodeInterface>();
                if (spellNode != null)
                {
                    spellNode.SetUsed(false);
                }
                Destroy(gameObject);
                return;
            }
            if (!isClone && pendingDropZone is NodeInventory targetInventory && inventoryClone != null)
            {
                var nodeInterface = GetComponent<SpellNodeInterface>();
                var cloneInterface = inventoryClone.GetComponent<SpellNodeInterface>();
                int cloneIndex = targetInventory.GetNodeIndex(cloneInterface);
                targetInventory.RemoveNodeFromInventory(cloneInterface);
                Destroy(inventoryClone.gameObject);
                inventoryClone = null;
                targetInventory.Receive(this);
                if (nodeInterface != null)
                {
                    targetInventory.InsertNodeAt(nodeInterface, cloneIndex);
                }
                return;
            }
            pendingDropZone.Receive(this);
        }
        else
        {
            if (isClone)
            {
                var source = inventorySource;
                var spellNode = source?.GetComponent<SpellNodeInterface>();
                if (spellNode != null)
                {
                    spellNode.SetUsed(false);
                }
                Destroy(gameObject);
                return;
            }
            if (inventory != null)
            {
                inventory.Receive(this);
                var nodeInterface = GetComponent<SpellNodeInterface>();
                if (nodeInterface != null)
                {
                    if (inventoryClone != null)
                    {
                        var cloneInterface = inventoryClone.GetComponent<SpellNodeInterface>();
                        int cloneIndex = inventory.GetNodeIndex(cloneInterface);
                        inventory.RemoveNodeFromInventory(cloneInterface);
                        Destroy(inventoryClone.gameObject);
                        inventoryClone = null;
                        inventory.InsertNodeAt(nodeInterface, cloneIndex);
                    }
                    else
                    {
                        inventory.InsertNodeAt(nodeInterface, savedListIndex);
                    }
                }
            }
            else // Se já está na grid e não foi dropado em nenhum lugar permitido, volta para a posição original
            {
                transform.SetParent(savedParent, true);
                rectTransform.anchoredPosition = savedPosition;
                OriginZone?.Receive(this);
            }
        }
        pendingDropZone = null;
    }

    private bool CanDrag()
    {
        if (!GameManager.Instance.uiController.playerUI.editSpellPanel.activeInHierarchy) return false;
        if (isClone) return false;
        return true;
    }
}