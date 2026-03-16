using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SyncedUIElement : NetworkBehaviour
{
    [ClientRpc]
    public void SyncText(string txt)
    {
        this.GetComponent<TextMeshProUGUI>().text = txt;
    }

    [ClientRpc]
    public void ShowOnlyForHost(bool interact)
    {
        if(!isServer)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
            if(this.TryGetComponent<Button>(out Button b))
            {
                b.interactable = interact;
            }
        }
    }
}
