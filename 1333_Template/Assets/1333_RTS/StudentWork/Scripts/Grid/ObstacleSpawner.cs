using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Spawns cuttable obstacles (trees, rocks) on free grid nodes and respawns them after a delay.
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GridManager gridManager;
    public GameObject[] obstaclePrefabs;
    public int maxTrees = 100;
    public float respawnDelay = 30f;

    private List<GameObject> activeObstacles = new();

    IEnumerator Start()
    {
        yield return new WaitUntil(() => gridManager != null && gridManager.IsInitialized);

        for (int i = 0; i < maxTrees; i++)
        {
            TrySpawnObstacles();
        }
    }

    // Attempts to place one obstacle on a random free node.
    void TrySpawnObstacles()
    {
        if (activeObstacles.Count >= maxTrees) return;

        GridNode node = GetRandomValidNode();
        if (node == null) return;

        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Vector3 pos = node.WorldPosition + Vector3.up * 0.1f;
        GameObject obstacle = Instantiate(prefab, pos, Quaternion.identity);

        obstacle.AddComponent<ObstacleCuttable>().Init(this, node);

        activeObstacles.Add(obstacle);
        node.IsOccupied = true;
        node.Walkable = false;
    }

    // Called by ObstacleCuttable when it is fully cut down.
    public void HandleCut(GameObject obstacle, GridNode node)
    {
        if (obstacle != null)
            Destroy(obstacle);

        if (node != null)
        {
            node.IsOccupied = false;
            node.Walkable = true;
        }

        activeObstacles.Remove(obstacle);
        StartCoroutine(RespawnAfterDelay());
    }

    // Wait for the delay, then spawn a new obstacle
    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        TrySpawnObstacles();
    }

    // Picks a random walkable, unoccupied node (up to 100 attempts).
    GridNode GetRandomValidNode()
    {
        var all = gridManager.GetAllNodes();
        int attempts = 0;

        while (attempts < 100)
        {
            var node = all[Random.Range(0, all.Count)];
            if (node.Walkable && !node.IsOccupied)
                return node;

            attempts++;
        }

        return null;
    }

}