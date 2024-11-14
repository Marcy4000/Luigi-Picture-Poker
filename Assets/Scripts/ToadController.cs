using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum ToadAnimation { Idle, Walk, Wave, Crawl, Cheer, Die, Dance1, Dance2, Dance3, Dance4 }

public class ToadController : MonoBehaviour
{
    [SerializeField] private Animator toadAnimator;
    [SerializeField] private NavMeshAgent toadAgent;

    private BoxCollider walkArea; // Define the walk area with a BoxCollider
    private Vector3 walkOffPoint; // Unique exit point

    ToadAnimation[] stationaryAnimations = { ToadAnimation.Wave, ToadAnimation.Cheer, ToadAnimation.Dance1, ToadAnimation.Dance2, ToadAnimation.Dance3, ToadAnimation.Dance4 };
    ToadAnimation[] walkAnimations = { ToadAnimation.Walk, ToadAnimation.Crawl };

    private Transform player;
    private bool hasReachedWalkPoint = false;
    private const int maxAttempts = 10; // Maximum attempts to find a valid point
    private bool isDoingBehavior = false;

    private void Start()
    {
        player = Camera.main.transform;
        walkArea = GameObject.Find("WalkArea").GetComponent<BoxCollider>(); // Find the walk area by name
        walkOffPoint = GameObject.Find("WalkOffPoint").transform.position; // Find the walk-off point by name
        SetRandomWalkPoint();

        Destroy(gameObject, 45.0f);
    }

    private void SetRandomWalkPoint()
    {
        // Pick a random, reachable point within the walk area bounds
        Vector3 randomPoint = GetRandomReachablePointInArea();
        if (randomPoint != Vector3.zero) // Ensure a valid point was found
        {
            toadAgent.SetDestination(randomPoint);
            toadAnimator.CrossFadeInFixedTime(ToadAnimation.Walk.ToString().ToLower(), 0.5f);
        }
        else
        {
            Debug.LogWarning("No reachable walk point found in area.");
        }
    }

    private Vector3 GetRandomReachablePointInArea()
    {
        Bounds bounds = walkArea.bounds;
        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate a random point within bounds
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 randomPoint = new Vector3(x, transform.position.y, z);

            bool isPointValid = true;

            // Check if point is on the NavMesh and reachable
            Collider[] colliders = Physics.OverlapSphere(randomPoint, 0.1f); // Check for obstacles at the point
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("BlockWalk"))
                {
                    isPointValid = false;
                    break;
                }
            }

            if (isPointValid)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 0.1f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }

        return Vector3.zero;
    }

    private void Update()
    {
        // Check if Toad has reached the walk point
        if (!hasReachedWalkPoint && !toadAgent.pathPending && toadAgent.remainingDistance <= toadAgent.stoppingDistance)
        {
            hasReachedWalkPoint = true;
            ChooseBehaviorAtWalkPoint();
        }

        // Check if Toad has reached the walk-off point and disable it
        if (hasReachedWalkPoint && !toadAgent.pathPending && toadAgent.remainingDistance <= toadAgent.stoppingDistance && !isDoingBehavior)
        {
            Destroy(gameObject); // Remove Toad from the scene
        }
    }

    private void ChooseBehaviorAtWalkPoint()
    {
        // Randomly select a behavior at the walk point
        float behaviorChance = Random.Range(0f, 1f);

        if (behaviorChance < 0.01f) // 1% chance to die (rarest)
        {
            toadAnimator.CrossFadeInFixedTime(ToadAnimation.Die.ToString().ToLower(), 0.5f);
            toadAgent.isStopped = true;
        }
        else if (behaviorChance < 0.10f) // 9% chance to crawl out
        {
            StartWalkingOff();
            toadAnimator.CrossFadeInFixedTime(ToadAnimation.Crawl.ToString().ToLower(), 0.5f);
        }
        else if (behaviorChance < 0.25f) // 15% chance of doing nothing
        {
            // No animation, simply stay at the walk point without doing anything
            toadAgent.isStopped = true;
        }
        else // 75% chance to stop and play a random animation
        {
            StartCoroutine(StopAndLookAtPlayer());
        }
    }


    private IEnumerator StopAndLookAtPlayer()
    {
        isDoingBehavior = true;
        toadAgent.isStopped = true;

        toadAnimator.CrossFadeInFixedTime(ToadAnimation.Idle.ToString().ToLower(), 0.5f);

        // Smoothly rotate to look at the player
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(player.position.x - transform.position.x, 0, player.position.z - transform.position.z));
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f); // Adjust speed as needed
            yield return null;
        }

        yield return new WaitForSeconds(0.7f); // Wait after rotation

        // Choose random animation and fixed duration
        ToadAnimation randomAnim = stationaryAnimations[Random.Range(0, stationaryAnimations.Length)];
        toadAnimator.CrossFadeInFixedTime(randomAnim.ToString().ToLower(), 0.5f);

        // Define fixed wait times for each animation
        float animWaitTime = randomAnim switch
        {
            ToadAnimation.Wave => 2.0f,
            ToadAnimation.Cheer => 2.5f,
            ToadAnimation.Dance1 => 7.02f,
            ToadAnimation.Dance2 => 5.5f,
            ToadAnimation.Dance3 => 16f,
            ToadAnimation.Dance4 => 21f,
            _ => 2.0f // Default wait time
        };

        yield return new WaitForSeconds(animWaitTime);

        StartWalkingOff();
        isDoingBehavior = false;
    }


    private void StartWalkingOff()
    {
        toadAgent.isStopped = false;

        // Randomly choose between walking (90%) and crawling (10%)
        if (Random.value < 0.05f) // 10% chance to crawl
        {
            toadAnimator.CrossFadeInFixedTime(ToadAnimation.Crawl.ToString().ToLower(), 0.5f);
        }
        else // 90% chance to walk
        {
            toadAnimator.CrossFadeInFixedTime(ToadAnimation.Walk.ToString().ToLower(), 0.5f);
        }

        toadAgent.SetDestination(walkOffPoint);
    }
}
