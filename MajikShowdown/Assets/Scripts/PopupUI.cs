using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopupUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string text;

    public bool isElement = false;

    private void Start()
    {
        Image image = GetComponent<Image>();
        if (image != null && !isElement)
        {
            image.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    private void OnDisable()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.Hide();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PopupManager.Instance.Show(text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PopupManager.Instance.Hide();
    }

    public void SetElementText(string newText)
    {
        text = newText;
    }
}