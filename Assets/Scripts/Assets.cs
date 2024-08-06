using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assets : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        GameObject player = GameObject.FindWithTag("Player");
        AssetPathfinding assetPathfinding = player.GetComponent<AssetPathfinding>();
        GameObject spawnerObject = GameObject.FindWithTag("Spawner");
        Spawner spawner = spawnerObject.GetComponent<Spawner>();
        var respawn = "";
        var time = 0;
        switch (gameObject.tag)
        {
            case "Potion":
                Destroy(gameObject);
                respawn = "Potion";
                time = 35;
                break;
            case "Garlic":
                Destroy(gameObject);
                respawn = "Garlic";
                time = 40;
                break;
            case "Pan":
                Destroy(gameObject);
                respawn = "Pan";
                time = 60;
                break;
            case "Donut":
                Destroy(gameObject);
                respawn = "Donut";
                time = 35;
                break;
            case "Coffee":
                Destroy(gameObject);
                respawn = "Coffee";
                time = 20;
                break;
            case "Cake":
                Destroy(gameObject);
                respawn = "Cake";
                time = 20;
                break;
            default:
                break;
        }
        Debug.Log("Destroyed: " + respawn);
        Debug.Log("Respawning: " + respawn + " in " + time + " seconds");
        spawner.RespawnAsset(respawn, time);
        assetPathfinding.dynamicAssets.Remove(gameObject);
        
    }
}
