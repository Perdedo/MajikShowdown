using Mirror;
using System.Collections;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemy;
    public Transform spawnPos;
    public float baseSpawnTime = 1.25f, minSpawnTime = 0.25f, spawnRateIncreaseTime = 15f, spawnRateIncrease = 0.05f, spawnerLifeTime = 300f;
    float spawnTime;
    bool isSpawning;
    GameObject aux;
    private void Start()
    {
        if(isServer)
        {
            isSpawning = true;
            spawnTime = baseSpawnTime;
            StartCoroutine(SpawnEnemy());
            StartCoroutine(IncreaseSpawnRate());
            StartCoroutine(StopSpawning());
        }
    }

    [Server]
    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(spawnTime);
        aux = Instantiate(enemy, spawnPos.position, Quaternion.identity);
        NetworkServer.Spawn(aux);
        if(isSpawning)
        {
            StartCoroutine(SpawnEnemy());
        }
    }

    [Server]
    IEnumerator IncreaseSpawnRate()
    {
        yield return new WaitForSeconds(spawnRateIncreaseTime);
        spawnTime = Mathf.Clamp(spawnTime - spawnRateIncrease, minSpawnTime, baseSpawnTime);
        if(isSpawning)
        {
            StartCoroutine(IncreaseSpawnRate());
        }
    }

    [Server]
    IEnumerator StopSpawning()
    {
        yield return new WaitForSeconds(spawnerLifeTime);
        isSpawning = false;
    }
}
