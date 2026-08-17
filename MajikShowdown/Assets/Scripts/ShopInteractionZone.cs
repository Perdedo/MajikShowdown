using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ShopInteractionZone : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    private void Awake()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        Rigidbody body = GetComponent<Rigidbody>();

        sphere.isTrigger = true;
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject.layer)) return;

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
        {
            Debug.LogWarning($"Player não encontrado a partir de {other.name}", other);
            return;
        }

        Transform playerRoot = player.transform.parent;

        if (playerRoot == null)
        {
            Debug.LogWarning($"O objeto {player.name} não possui um objeto pai.", player);
            return;
        }

        PlayerUI playerUI = playerRoot.GetComponentInChildren<PlayerUI>(true);

        if (playerUI == null)
        {
            Debug.LogWarning($"PlayerUI não encontrado dentro de {playerRoot.name}.", playerRoot);
            return;
        }

        Debug.Log($"Player entrou na loja: {playerRoot.name}", playerRoot);
        playerUI.EnterShopZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject.layer)) return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null || player.transform.parent == null) return;

        PlayerUI playerUI = player.transform.parent.GetComponentInChildren<PlayerUI>(true);
        if (playerUI == null) return;

        Debug.Log($"Player saiu da loja: {player.transform.parent.name}", player);
        playerUI.ExitShopZone(this);
    }

    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }
}