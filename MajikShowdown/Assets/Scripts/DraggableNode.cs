using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public CanvasGroup canvasGroup;
    int savedListIndex = 0;

    public IDropZone OriginZone { get; private set; }
    private IDropZone pendingDropZone;

    public bool isClone = false;
    public DraggableNode inventorySource;

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
            clone.SetOriginZone(null);
            clone.canvas = canvas;
            clone.rectTransform = cloneGO.GetComponent<RectTransform>();
            clone.canvasGroup = cloneGO.GetComponent<CanvasGroup>();
            clone.canvasGroup.alpha = 1f;
            clone.canvasGroup.blocksRaycasts = true;

            var cloneInterface = cloneGO.GetComponent<SpellNodeInterface>();
            if (cloneInterface != null)
            {
                cloneInterface.linkedDescription = GameManager.Instance.uiController.playerUI.spellNodeDescription;
            }

            pendingDropZone.Receive(clone);
            inventory.Receive(this);
            var nodeInterface = GetComponent<SpellNodeInterface>();
            if (nodeInterface != null)
            {
                inventory.InsertNodeAt(nodeInterface, savedListIndex);
            }
            var spellNode = GetComponent<SpellNodeInterface>();
            if (spellNode != null)
            {
                GameManager.Instance.uiController.playerUI.caster.SetNodeInUse(spellNode.Node, true);
            }
        }
        else if (pendingDropZone != null)
        {
            if (isClone && pendingDropZone is NodeInventory)
            {
                var source = inventorySource;
                var spellNode = source.GetComponent<SpellNodeInterface>();
                if (spellNode != null)
                {
                    GameManager.Instance.uiController.playerUI.caster.SetNodeInUse(spellNode.Node, false);
                }
                Destroy(gameObject);
                return;
            }
            pendingDropZone.Receive(this);
        }
        else
        {
            if (isClone)
            {
                var source = inventorySource;
                var spellNode = source.GetComponent<SpellNodeInterface>();
                if (spellNode != null)
                {
                    GameManager.Instance.uiController.playerUI.caster.SetNodeInUse(spellNode.Node, false);
                }
                Destroy(gameObject);
                return;
            }
            inventory.Receive(this);
            var nodeInterface = GetComponent<SpellNodeInterface>();
            if (nodeInterface != null)
            {
                inventory.InsertNodeAt(nodeInterface, savedListIndex);
            }
        }
        pendingDropZone = null;
    }

    private bool CanDrag()
    {
        if (!GameManager.Instance.uiController.playerUI.editSpellPanel.activeInHierarchy) return false;
        if (!isClone)
        {
            var node = GetComponent<SpellNodeInterface>();
            if (node != null && node.Node.IsInUse) return false;
        }
        return true;
    }
}