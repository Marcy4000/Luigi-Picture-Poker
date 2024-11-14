using UnityEngine;
using System.Collections;

public class ToadSpawner : MonoBehaviour
{
    [SerializeField] private GameObject toadPrefab;
    [SerializeField] private BoxCollider spawnArea; // Define the spawn area with a BoxCollider

    private void Start()
    {
        StartCoroutine(SpawnToadAtRandomIntervals());
    }

    private IEnumerator SpawnToadAtRandomIntervals()
    {
        while (true)
        {
            float waitTime = Random.Range(25f, 35f);
            yield return new WaitForSeconds(waitTime);
            SpawnToad();
        }
    }

    public void SpawnToad()
    {
        Vector3 spawnPosition = GetRandomPointInArea();
        Instantiate(toadPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPointInArea()
    {
        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, 0, z); // Adjust y as needed
    }
}
