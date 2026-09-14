using UnityEngine;

public class TrainingEnemySpawner : InteractableObject
{
    public GameObject enemyPrefab;
    public Transform spawnPos;
    public int spawnCount;
    public Vector3 distanceBetweenSpawns;
    public override void Interact(Player player)
    {
        GameManager.Instance.trainingController.SpawnEnemy(this, enemyPrefab, spawnPos.position, spawnCount, distanceBetweenSpawns);
    }
}
