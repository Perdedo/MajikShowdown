using UnityEngine;

public class TrainingEnemySpawner : InteractableObject
{
    [SerializeField] private Collider interactionArea;

    public GameObject enemyPrefab;
    public Transform spawnPos;
    public int spawnCount;
    public Vector3 distanceBetweenSpawns;

    public override void Interact(Player player)
    {
        if (player == null) return;

        GameManager.Instance.trainingController.SpawnEnemy(
            this,
            enemyPrefab,
            spawnPos.position,
            spawnCount,
            distanceBetweenSpawns
        );
    }

    public override void CheckForPlayer()
    {
        if (interactionArea == null) return;

        foreach (Player player in GameManager.Instance.Players)
        {
            Vector3 closestPoint = interactionArea.ClosestPoint(player.transform.position);
            float distance = Vector3.Distance(player.transform.position, closestPoint);

            if (distance <= GameManager.Instance.interactionRadius)
            {
                if (player.currentInteraction == null)
                {
                    player.currentInteraction = this;
                }
            }
            else if (player.currentInteraction == this)
            {
                player.currentInteraction = null;
            }
        }
    }
}