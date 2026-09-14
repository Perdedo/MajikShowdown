using UnityEngine;

public class TrainingEnemySpawner : InteractableObject
{
    public GameObject enemyPrefab;
    public Transform spawnPos;
    public int spawnCount;
    public Vector3 distanceBetweenSpawns;
    public float interactRadius = 5;
    public override void Interact(Player player)
    {
        GameManager.Instance.trainingController.SpawnEnemy(this, enemyPrefab, spawnPos.position, spawnCount, distanceBetweenSpawns);
    }

    public override void CheckForPlayer()
    {
        foreach (Player p in GameManager.Instance.Players)
        {
            float dist = Vector3.Distance(
                p.transform.position,
                transform.position
            );

            if (dist <= interactRadius)
            {
                if (p.currentInteraction == null ||
                    dist < Vector3.Distance(
                        p.transform.position,
                        p.currentInteraction.transform.position
                    ))
                {
                    p.currentInteraction = this;
                }
            }
            else if (p.currentInteraction == this)
            {
                p.currentInteraction = null;
            }
        }
    }
}
