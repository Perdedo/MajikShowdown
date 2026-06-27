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
        if (GameManager.Instance.uiController.playerUI.popupManager != null)
        {
            GameManager.Instance.uiController.playerUI.popupManager.Hide();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.Instance.uiController.playerUI.popupManager.Show(text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.uiController.playerUI.popupManager.Hide();
    }

    public void SetElementText(string newText)
    {
        text = newText;
    }
}