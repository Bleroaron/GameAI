
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    public TerrainGeneration terrainGenerator;

    [Header("Player")]
    public GameObject player;

    [Header("Assets")]
    public GameObject[] assetsPrefab;
    public List<GameObject> spawnedAssets = new List<GameObject>();

    [Header("Enemies")]
    public GameObject[] enemies;

    [Header("Options")]
    public int maxPlacemntTries;
    public LayerMask groundLayer;
    private Dictionary<string, Coroutine> respawnCoroutines = new Dictionary<string, Coroutine>();


    public void SpawnPlayerAssetsEnemiesOnNavMesh()
    {
        if (player != null || assetsPrefab != null || enemies != null)
        {
            Vector3 spawnPosition = GetRandomPointOnNavMesh();
            player.transform.position = spawnPosition;

            for (int i = 0; i < assetsPrefab.Length; i++)
            {
                spawnPosition = GetRandomPointOnNavMesh();
                
                var asset = Instantiate(assetsPrefab[i], spawnPosition + new Vector3(0,0,0), Quaternion.identity);

                GameObject player = GameObject.FindWithTag("Player");
                AssetPathfinding assetPathfinding = player.GetComponent<AssetPathfinding>();

                assetPathfinding.dynamicAssets.Add(asset);
                
                spawnedAssets.Add(asset);
            }
            for (int i = 0; i < enemies.Length; i++)
            {
                 spawnPosition = GetRandomPointOnNavMesh();
                enemies[i].transform.position = spawnPosition;
              
            }
           

        }
        else
        {
            Debug.LogError("Not all prefabs are assigned.");
        }
    }
    public Vector3 GetRandomPointOnNavMesh()
    {
        float randomX = Random.Range(-terrainGenerator.Width / 2, terrainGenerator.Width/2);
        float randomZ = Random.Range(-terrainGenerator.Length / 2, terrainGenerator.Length/2);
        Vector3 startingPoint = new Vector3(randomX, 0, randomZ);

        if (NavMesh.SamplePosition(startingPoint, out NavMeshHit hit, 20, NavMesh.AllAreas))
        {
            RaycastHit terrainHit;
            if (Physics.Raycast(new Vector3(hit.position.x, terrainGenerator.maxHeight, hit.position.z), Vector3.down, out terrainHit))
            {
                return new Vector3(hit.position.x, terrainHit.point.y +0.1f, hit.position.z);
            }
        }

        return Vector3.zero;
    }
    
    public void RespawnAsset(string asset, float assetRespawnDelay = 10)

    {
        GameObject assetPrefab = FindAssetByTag(asset);
        spawnedAssets.Remove(spawnedAssets.FirstOrDefault(spawned => spawned.CompareTag(asset)));
        if (assetPrefab != null)
        {
            if (respawnCoroutines.ContainsKey(asset) && respawnCoroutines[asset] != null)
            {
                Debug.Log("Respawn coroutine already running for: " + asset);
            }
            else
            {
                Coroutine coroutine = StartCoroutine(RespawnAssetCoroutine(assetPrefab, assetRespawnDelay));
                respawnCoroutines[asset] = coroutine;
            }
        }
        else
        {
            Debug.LogError("Asset not found: " + asset);
        }
    }

    private IEnumerator RespawnAssetCoroutine(GameObject assetPrefab, float assetRespawnDelay)
    {
       
        yield return new WaitForSeconds(assetRespawnDelay);

        Vector3 spawnPosition = GetRandomPointOnNavMesh();
        GameObject respawnedAsset = Instantiate(assetPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Respawned: " + assetPrefab.tag);
       
        spawnedAssets.Add(respawnedAsset);
    }

    private GameObject FindAssetByTag(string tag)
    {
        foreach (var assetPrefab in assetsPrefab)
        {
            if (assetPrefab.CompareTag(tag))
            {
                return assetPrefab;
            }
        }
        return null;
    }
    
}
