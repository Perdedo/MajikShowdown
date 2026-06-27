using Mirror;
using TMPro;
using UnityEngine;

public class PopupManager : NetworkBehaviour
{
    //public static PopupManager Instance;

    [SerializeField] private GameObject popup;
    [SerializeField] private Vector2 popupOffset;

    private TextMeshProUGUI popupText;

    public bool network = true;

    /*private void Awake()
    {
        Instance = this;
    }*/

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
        if (!isLocalPlayer && network) return;
        if (popupText == null) return;
        popupText.text = text;
        popup.SetActive(true);
    }

    public void Hide()
    {
        if (!isLocalPlayer && network) return;
        popup.SetActive(false);
    }
}