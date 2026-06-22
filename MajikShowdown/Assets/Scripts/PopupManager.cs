using Mirror;
using TMPro;
using UnityEngine;

public class PopupManager : NetworkBehaviour
{
    public static PopupManager Instance;

    [SerializeField] private GameObject popup;
    [SerializeField] private Vector2 popupOffset;

    private TextMeshProUGUI popupText;

    public bool network = true;

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
        if(network)
        {
            RPCShow();
        }
        else
        {
            popup.SetActive(true);
        }
    }

    [TargetRpc]
    public void RPCShow()
    {
        popup.SetActive(true);
    }

    public void Hide()
    {
        if(network)
        {
            RPCHide();
        }
        else
        {
            popup.SetActive(false);
        }
    }

    [TargetRpc]
    public void RPCHide()
    {
        popup.SetActive(false);
    }
}