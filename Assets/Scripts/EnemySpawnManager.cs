using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy spawning from scanned room surfaces (walls and ceilings).
/// </summary>
public class EnemySpawnManager : MonoBehaviour
{
    [Header("Dependencies")]
    public ARSceneManager arSceneManager;
    
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Array for variety
    public GameObject wallHolePrefab; // Visual effect for a hole in the wall
    public GameObject ceilingBreakthroughPrefab; // Visual effect for ceiling breakthrough

    [Header("Spawning Logic")]
    public float spawnInterval = 5f;
    public int maxEnemies = 5;
    public float holeDuration = 3f;

    private List<GameObject> _activeEnemies = new List<GameObject>();
    private bool _isSpawning = false;
    private int _enemiesRemainingToSpawn = 0;
    private int _totalEnemiesInWave = 0;

    public int GetRemainingEnemiesCount()
    {
        _activeEnemies.RemoveAll(e => e == null);
        return _enemiesRemainingToSpawn + _activeEnemies.Count;
    }

    private void Start()
    {
        if (arSceneManager == null)
            arSceneManager = FindFirstObjectByType<ARSceneManager>();

        // We no longer start spawning on scene load, ARGameManager will handle it.
    }

    private void OnDestroy()
    {
    }

    public void StartWave(int count)
    {
        _totalEnemiesInWave = count;
        _enemiesRemainingToSpawn = count;
        _activeEnemies.Clear();
        
        if (!_isSpawning)
        {
            _isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
        
        Debug.Log($"[SAO] Wave started with {count} enemies.");
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            _activeEnemies.RemoveAll(e => e == null);

            if (_enemiesRemainingToSpawn > 0 && _activeEnemies.Count < maxEnemies)
            {
                bool spawnFromCeiling = Random.value > 0.7f; 
                if (spawnFromCeiling)
                {
                    SpawnEnemyFromCeiling();
                }
                else
                {
                    SpawnEnemyFromWall();
                }
                _enemiesRemainingToSpawn--;
            }
            else if (_enemiesRemainingToSpawn <= 0 && _activeEnemies.Count == 0)
            {
                _isSpawning = false;
                if (ARGameManager.Instance != null)
                {
                    ARGameManager.Instance.OnWaveCleared();
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemyFromWall()
    {
        Vector3 spawnPos = arSceneManager.GetRandomPointOnWall();
        if (spawnPos == Vector3.zero) return;

        // Find the wall plane to get the correct rotation (facing away from the wall)
        // We can do a quick raycast or just use the plane's forward if we had it stored.
        // For simplicity, we'll look for the nearest wall plane.
        var walls = arSceneManager.Walls;
        OVRScenePlane nearestWall = null;
        float minDist = float.MaxValue;
        foreach(var wall in walls)
        {
            float dist = Vector3.Distance(spawnPos, wall.transform.position);
            if(dist < minDist)
            {
                minDist = dist;
                nearestWall = wall;
            }
        }

        Quaternion rotation = nearestWall != null ? nearestWall.transform.rotation : Quaternion.identity;
        
        // Spawn hole effect
        if (wallHolePrefab != null)
        {
            GameObject hole = Instantiate(wallHolePrefab, spawnPos, rotation);
            Destroy(hole, holeDuration);
        }

        // Spawn enemy slightly in front of the wall
        Vector3 enemyPos = spawnPos + (rotation * Vector3.forward * 0.2f);
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemy = Instantiate(prefab, enemyPos, rotation);
        InitializeEnemy(enemy);
        _activeEnemies.Add(enemy);
        
        Debug.Log("[SAO] Enemy spawned from wall.");
    }

    private void SpawnEnemyFromCeiling()
    {
        Vector3 spawnPos = arSceneManager.GetRandomPointOnCeiling();
        if (spawnPos == Vector3.zero) return;

        // Ceiling normal is usually down (transform.up or -transform.up depending on how it's generated)
        // In Meta Scene SDK, Plane transform.up is usually the normal.
        
        if (ceilingBreakthroughPrefab != null)
        {
            GameObject breakthrough = Instantiate(ceilingBreakthroughPrefab, spawnPos, Quaternion.LookRotation(Vector3.down));
            Destroy(breakthrough, holeDuration);
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        InitializeEnemy(enemy);
        _activeEnemies.Add(enemy);
        
        Debug.Log("[SAO] Enemy spawned from ceiling.");
    }

    private void InitializeEnemy(GameObject enemy)
    {
        // Add AI and Stats if not present on prefab
        if (enemy.GetComponent<EnemyAI>() == null) enemy.AddComponent<EnemyAI>();
        if (enemy.GetComponent<EnemyStats>() == null) enemy.AddComponent<EnemyStats>();
        
        // Ensure tag is set for sword trigger
        enemy.tag = "Enemy";
    }
}
