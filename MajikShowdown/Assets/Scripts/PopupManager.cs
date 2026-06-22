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
        if(network && !isServer)
        {
            CMDShow(text);
        }
        else
        {
            popupText.text = text;
            popup.SetActive(true);
        }
    }

    [Command]
    public void CMDShow(string text)
    {
        RPCShow(this.connectionToClient, text);
    }
    [TargetRpc]
    public void RPCShow(NetworkConnection target, string text)
    {
        popupText.text = text;
        popup.SetActive(true);
    }

    public void Hide()
    {
        if(network && !isServer)
        {
            CMDHide();
        }
        else
        {
            popup.SetActive(false);
        }
    }

    [Command]
    public void CMDHide()
    {
        RPCHide(this.connectionToClient);
    }


    [TargetRpc]
    public void RPCHide(NetworkConnection target)
    {
        popup.SetActive(false);
    }
}