using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public CanvasGroup canvasGroup;
    [HideInInspector] public int acquisitionOrder;

    private int savedListIndex;
    private Vector2 savedPosition;
    private Vector3 savedWorldPosition;
    private Transform savedParent;
    private NodeTween nodeTween;

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
        nodeTween = GetComponent<NodeTween>();
    }

    public void SetOriginZone(IDropZone zone)
    {
        OriginZone = zone;
    }

    public void RegisterDrop(IDropZone dropZone)
    {
        pendingDropZone = dropZone;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;

        nodeTween?.Stop();
        pendingDropZone = null;
        canvas = GetComponentInParent<Canvas>();
        savedPosition = rectTransform.anchoredPosition;
        savedWorldPosition = rectTransform.position;
        savedParent = transform.parent;

        SpellNodeInterface nodeInterface = GetComponent<SpellNodeInterface>();
        nodeInterface?.SelectOnly();

        NodeInventory inventory = OriginZone as NodeInventory;

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
        transform.SetAsLastSibling();

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanDrag()) return;

        bool startedFromGrid = OriginZone is HexGridNode;
        Vector3 releasedWorldPosition = rectTransform.position;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        NodeInventory inventory = OriginZone as NodeInventory;
        bool droppedOnSameInventory = pendingDropZone != null && ReferenceEquals(pendingDropZone, inventory);
        bool shouldReturnToInventory = inventory != null && !isClone && (pendingDropZone == null || droppedOnSameInventory);

        if (shouldReturnToInventory)
        {
            ReturnToInventory(inventory);
            return;
        }

        if (startedFromGrid && pendingDropZone is NodeInventory targetInventory && inventoryClone != null)
        {
            ReturnFromGridToInventory(targetInventory);
            return;
        }

        ResolveDrop(inventory);
        inventory?.Unfreeze();

        bool endedInGrid = OriginZone is HexGridNode;

        if (startedFromGrid || endedInGrid)
        {
            nodeTween?.SlideFrom(releasedWorldPosition);
        }
    }

    private void ReturnToInventory(NodeInventory inventory)
    {
        pendingDropZone = null;
        canvasGroup.blocksRaycasts = false;

        if (nodeTween != null)
        {
            nodeTween.SlideToWorld(savedWorldPosition, () =>
            {
                transform.SetParent(savedParent, true);
                transform.SetSiblingIndex(savedListIndex);
                rectTransform.anchoredPosition = savedPosition;
                canvasGroup.blocksRaycasts = true;
                inventory.Unfreeze();
            });
        }
        else
        {
            transform.SetParent(savedParent, true);
            transform.SetSiblingIndex(savedListIndex);
            rectTransform.anchoredPosition = savedPosition;
            canvasGroup.blocksRaycasts = true;
            inventory.Unfreeze();
        }
    }

    private void ReturnFromGridToInventory(NodeInventory targetInventory)
    {
        SpellNodeInterface nodeInterface = GetComponent<SpellNodeInterface>();
        SpellNodeInterface cloneInterface = inventoryClone.GetComponent<SpellNodeInterface>();
        int cloneIndex = targetInventory.GetNodeIndex(cloneInterface);
        Vector3 targetWorldPosition = inventoryClone.rectTransform.position;
        DraggableNode savedClone = inventoryClone;

        pendingDropZone = null;
        canvasGroup.blocksRaycasts = false;

        if (nodeTween != null)
        {
            nodeTween.SlideToWorld(targetWorldPosition, () =>
            {
                CompleteGridToInventoryReturn(targetInventory, nodeInterface, cloneInterface, savedClone, cloneIndex);
            });
        }
        else
        {
            CompleteGridToInventoryReturn(targetInventory, nodeInterface, cloneInterface, savedClone, cloneIndex);
        }
    }

    private void CompleteGridToInventoryReturn(NodeInventory targetInventory, SpellNodeInterface nodeInterface, SpellNodeInterface cloneInterface, DraggableNode savedClone, int cloneIndex)
    {
        targetInventory.RemoveNodeFromInventory(cloneInterface);
        savedClone.gameObject.SetActive(false);
        Destroy(savedClone.gameObject);

        inventoryClone = null;

        targetInventory.Receive(this);

        if (nodeInterface != null)
        {
            targetInventory.InsertNodeAt(nodeInterface, cloneIndex);
        }

        canvasGroup.blocksRaycasts = true;
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

            SpellNodeInterface cloneInterface = cloneGO.GetComponent<SpellNodeInterface>();

            inventoryClone = clone;
            inventory.Receive(clone);
            inventory.InsertNodeAt(cloneInterface, savedListIndex);
            pendingDropZone.Receive(this);

            if (cloneInterface != null)
            {
                cloneInterface.SetUsed(true);
                SpellNodeInterface currentInterface = GetComponent<SpellNodeInterface>();
                currentInterface?.ApplyUsedVisual(false);
            }
        }
        else if (pendingDropZone != null)
        {
            if (isClone && pendingDropZone is NodeInventory)
            {
                DraggableNode source = inventorySource;
                SpellNodeInterface spellNode = source?.GetComponent<SpellNodeInterface>();

                if (spellNode != null)
                {
                    spellNode.SetUsed(false);
                }

                Destroy(gameObject);
                return;
            }

            if (!isClone && pendingDropZone is NodeInventory targetInventory && inventoryClone != null)
            {
                SpellNodeInterface nodeInterface = GetComponent<SpellNodeInterface>();
                SpellNodeInterface cloneInterface = inventoryClone.GetComponent<SpellNodeInterface>();
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
                DraggableNode source = inventorySource;
                SpellNodeInterface spellNode = source?.GetComponent<SpellNodeInterface>();

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
                SpellNodeInterface nodeInterface = GetComponent<SpellNodeInterface>();

                if (nodeInterface != null)
                {
                    if (inventoryClone != null)
                    {
                        SpellNodeInterface cloneInterface = inventoryClone.GetComponent<SpellNodeInterface>();
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
            else
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