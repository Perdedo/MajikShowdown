using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopupUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string text;

    [Header("Popup")]
    public GameObject popup;
    public Vector2 popupOffsetPos;
    TextMeshProUGUI popupText;
    public bool isElementPopup;


    void Start()
    {
        if (popup == null) return;

        popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (popupText != null)
        {
            if(isElementPopup)
            {
                popupText.text = "bla";
            }
            else
            {
                popupText.text = text;
            }
        }
        this.gameObject.GetComponent<Image>().color = Color.white * 1.5f;
        popup.SetActive(false);
    }

    void Update()
    {
        if (popup.activeSelf)
        {
            popup.transform.position = Input.mousePosition + (Vector3)popupOffsetPos;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowPopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HidePopup();
    }

    void OnDisable()
    {
        HidePopup();
    }

    void ShowPopup()
    {
        popup.SetActive(true);
    }

    void HidePopup()
    {
        popup.SetActive(false);
    }
}
