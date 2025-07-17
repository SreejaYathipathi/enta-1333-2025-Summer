using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GridManager gridManager;
    public GameObject[] obstaclePrefabs;
    public int maxTrees = 100;
    public float respawnDelay = 30f;

    private List<GameObject> activeObstacles = new();

    void Start()
    {
        for (int i = 0; i < maxTrees; i++)
        {
            TrySpawnObstacles();
        }
    }

    void TrySpawnObstacles()
    {
        if (activeObstacles.Count >= maxTrees) return;

        GridNode node = GetRandomValidNode();
        if (node == null) return;

        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Vector3 pos = node.WorldPosition + Vector3.up * 0.1f;
        GameObject obstacle = Instantiate(prefab, pos, Quaternion.identity);

        /*Renderer rend = prefab.GetComponentInChildren<Renderer>();
        float offset = rend ? rend.bounds.extents.y : 0.5f;
        Vector3 pos = node.WorldPosition + Vector3.up * offset;

        GameObject obstacle = Instantiate(prefab, pos, Quaternion.identity);*/

        // Attach cuttable behavior directly
        obstacle.AddComponent<ObstacleCuttable>().Init(this, node);

        activeObstacles.Add(obstacle);
        node.IsOccupied = true;
        node.Walkable = false;
    }

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

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        TrySpawnObstacles();
    }

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