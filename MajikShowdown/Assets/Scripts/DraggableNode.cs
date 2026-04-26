using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public IDropZone OriginZone { get; private set; }
    private IDropZone pendingDropZone;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
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
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        pendingDropZone = null;
        var node = GetComponent<SpellNodeInterface>();
        node?.SelectOnly();
        OriginZone?.Release(this);
        var inventory = OriginZone as NodeInventory;
        inventory?.Freeze();
        transform.SetParent(canvas.transform, true);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        var inventory = OriginZone as NodeInventory;
        ResolveDrop();
        inventory?.Unfreeze();
    }

    private void ResolveDrop()
    {
        if (pendingDropZone != null)
        {
            pendingDropZone.Receive(this);
        }
        else
        {
            OriginZone?.Receive(this);
        }
        pendingDropZone = null;
    }
}