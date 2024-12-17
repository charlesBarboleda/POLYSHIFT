using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    [System.Serializable]
    public class EnemySpawnData
    {
        public string EnemyName;
        public float Probability;
    }

    [System.Serializable]
    public class LevelSpawnConfig
    {
        public int MinLevel;
        public int MaxLevel;
        public List<EnemySpawnData> EnemiesWithProbability;
    }

    public List<LevelSpawnConfig> SpawnConfigs = new List<LevelSpawnConfig>();
    public float SpawnRate = 1f;
    public int MaxEnemies = 100;
    public int EnemiesToSpawn = 0;
    public bool IsSpawning = false;
    public List<Transform> spawnPositions = new List<Transform>();

    private Dictionary<string, float> currentSpawnProbabilities = new Dictionary<string, float>();

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (IsServer)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                // Spawn a ZombieSmall
                GameObject zombieSmall = ObjectPooler.Instance.Spawn("ZombieSmall", Vector3.zero, Quaternion.identity);
                if (zombieSmall.TryGetComponent(out NetworkObject networkObject))
                {
                    networkObject.Spawn();
                }
            }
            if (GameManager.Instance.SpawnedEnemies.Count < MaxEnemies && EnemiesToSpawn > 0)
            {
                if (!IsSpawning)
                {
                    StartCoroutine(SpawnEnemiesCoroutine());
                }
            }

        }


    }

    public void KillAllAllies()
    {
        if (!IsServer) return;

        List<Enemy> spawnedAlliesCopy = new List<Enemy>(GameManager.Instance.SpawnedEnemies);

        const int LethalDamage = 999999;

        foreach (var ally in spawnedAlliesCopy)
        {
            if (ally != null && ally.TryGetComponent(out IDamageable networkHealth))
            {
                networkHealth.RequestTakeDamageServerRpc(LethalDamage, 0);
            }
        }
    }

    public void KillAllEnemies()
    {
        if (!IsServer) return;

        List<Enemy> spawnedEnemiesCopy = new List<Enemy>(GameManager.Instance.SpawnedEnemies);
        const int LethalDamage = 999999;

        foreach (var enemy in spawnedEnemiesCopy)
        {
            if (enemy != null && enemy.TryGetComponent(out EnemyNetworkHealth enemyHealth))
            {
                enemyHealth.RequestTakeDamageServerRpc(LethalDamage, 0);
            }
        }

        EnemiesToSpawn = 0;
    }


    public void SpawnEnemies()
    {
        SetEnemies();
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    void SetEnemies()
    {
        int gameLevel = GameManager.Instance.GameLevel.Value;
        int playersAlive = GameManager.Instance.AlivePlayers.Count;
        Debug.Log("Trying to spawn enemies");
        EnemiesToSpawn = gameLevel * 10 * playersAlive;
        Debug.Log($"Spawning {EnemiesToSpawn} enemies for {playersAlive} players at level {gameLevel}");
        SpawnRate = Mathf.Max(0.1f, 2f - gameLevel * 0.1f);

        UpdateSpawnProbabilitiesForLevel(gameLevel);
    }

    void UpdateSpawnProbabilitiesForLevel(int gameLevel)
    {
        currentSpawnProbabilities.Clear();

        foreach (var config in SpawnConfigs)
        {
            if (gameLevel >= config.MinLevel && gameLevel <= config.MaxLevel)
            {
                foreach (var enemy in config.EnemiesWithProbability)
                {
                    if (!currentSpawnProbabilities.ContainsKey(enemy.EnemyName))
                    {
                        currentSpawnProbabilities[enemy.EnemyName] = enemy.Probability;
                    }
                }
            }
        }
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        IsSpawning = true;
        while (EnemiesToSpawn > 0 && GameManager.Instance.SpawnedEnemies.Count <= MaxEnemies)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(SpawnRate);
        }
        IsSpawning = false;
    }

    void SpawnEnemy()
    {
        if (currentSpawnProbabilities.Count == 0)
        {
            Debug.LogError("No enemies to spawn for the current level");
            return;
        }

        string enemyToSpawn = GetRandomEnemyByProbability();
        if (enemyToSpawn == null)
        {
            Debug.LogWarning("Enemy spawn failed due to probability mismatch");
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = ObjectPooler.Instance.Spawn(enemyToSpawn, spawnPosition, Quaternion.identity);


        if (enemy.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
        }

        EnemiesToSpawn--;
    }

    string GetRandomEnemyByProbability()
    {
        float totalProbability = currentSpawnProbabilities.Values.Sum();
        float randomPoint = Random.Range(0, totalProbability);

        float cumulativeProbability = 0f;
        foreach (var kvp in currentSpawnProbabilities)
        {
            cumulativeProbability += kvp.Value;
            if (randomPoint <= cumulativeProbability)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Count)].position;
        return spawnPosition;
    }
}
