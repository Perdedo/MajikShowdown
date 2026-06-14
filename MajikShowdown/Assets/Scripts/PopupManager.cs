using TMPro;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [SerializeField] private GameObject popup;
    [SerializeField] private Vector2 popupOffset;

    private TextMeshProUGUI popupText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        popupText.raycastTarget = false;
        popup.SetActive(false);
    }

    private void Update()
    {
        if (popup.activeSelf)
        {
            popup.transform.position =
                Input.mousePosition + (Vector3)popupOffset;
        }
    }

    public void Show(string text)
    {
        if (popupText == null) return;
        popupText.text = text;
        popup.SetActive(true);
    }

    public void Hide()
    {
        popup.SetActive(false);
    }
}