
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SpectatorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player owner;
    [SerializeField] private LookAnchor cameraLookAnchor;

    private PlayerUI playerUI;
    private Player currentTarget;

    private bool isSpectating;

    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInChildren<Player>(true);
        }

        if (cameraLookAnchor == null)
        {
            cameraLookAnchor = GetComponentInChildren<LookAnchor>(true);
        }
    }

    private void Update()
    {
        if (owner == null) return;

        if (owner.network && !owner.isLocalPlayer) return;

        if (GameManager.Instance == null) return;

        if (playerUI == null)
        {
            playerUI = GameManager.Instance.uiController.playerUI;
        }

        if (owner.dead)
        {
            if (!isSpectating)
            {
                StartSpectating();
            }

            if (isSpectating)
            {
                CheckCurrentTarget();
                HandleSpectatorInput();
            }
        }
        else if (isSpectating)
        {
            StopSpectating();
        }
    }

    private void StartSpectating()
    {
        if (cameraLookAnchor == null || owner.lookAnchor == null)
        {
            Debug.LogWarning("[Spectator] Camera references are missing.");
            return;
        }

        isSpectating = true;
        currentTarget = null;

        FindNextAlivePlayer(0);

        if (playerUI != null)
        {
            playerUI.ShowSpectatorUI(GetTargetName(currentTarget));
        }

        Debug.Log("[Spectator] Spectator mode activated.");
    }

    private void StopSpectating()
    {
        isSpectating = false;
        currentTarget = null;

        if (cameraLookAnchor != null && owner.lookAnchor != null)
        {
            cameraLookAnchor.PlayerFollow = owner.lookAnchor;
        }

        if (playerUI != null)
        {
            playerUI.HideSpectatorUI();
        }

        Debug.Log("[Spectator] Spectator mode deactivated.");
    }

    private void CheckCurrentTarget()
    {
        if (currentTarget != null && !currentTarget.dead)
        {
            return;
        }

        int nextIndex = currentTarget != null ? GameManager.Instance.Players.IndexOf(currentTarget) + 1 : 0;

        currentTarget = null;

        FindNextAlivePlayer(nextIndex);
    }

    private void HandleSpectatorInput()
    {
        if (playerUI == null || !playerUI.inGame) return;

        if (Mouse.current == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        int nextIndex = currentTarget != null ? GameManager.Instance.Players.IndexOf(currentTarget) + 1 : 0;

        FindNextAlivePlayer(nextIndex);
    }

    private void FindNextAlivePlayer(int startIndex)
    {
        var players = GameManager.Instance.Players;

        if (players == null || players.Count == 0)
        {
            SetTarget(null);
            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            int index = (startIndex + i) % players.Count;

            Player candidate = players[index];

            if (candidate == null || candidate == owner || candidate.dead)
            {
                continue;
            }

            if (candidate.lookAnchor == null)
            {
                continue;
            }

            SetTarget(candidate);
            return;
        }

        SetTarget(null);
    }

    private void SetTarget(Player target)
    {
        currentTarget = target;

        if (target != null && cameraLookAnchor != null)
        {
            cameraLookAnchor.PlayerFollow = target.lookAnchor;
        }

        if (playerUI != null)
        {
            playerUI.UpdateSpectatorUI(GetTargetName(target));
        }
    }

    private string GetTargetName(Player target)
    {
        if (target == null)
        {
            return null;
        }

        int index = GameManager.Instance.Players.IndexOf(target);

        return "Player " + (index + 1);
    }
}
