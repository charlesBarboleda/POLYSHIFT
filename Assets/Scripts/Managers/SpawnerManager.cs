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
        EnemiesToSpawn = gameLevel * 10 * playersAlive;
        SpawnRate = Mathf.Max(0.01f / 7f, 2f - gameLevel * 0.1f);

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
        while (EnemiesToSpawn > 0 && GameManager.Instance.SpawnedEnemies.Count <= MaxEnemies)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(SpawnRate);
        }
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
