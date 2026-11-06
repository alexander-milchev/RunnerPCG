using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Boid boidPrefab;
    [Header("Weights")]
    [SerializeField] private float cohesionWeight = 5f;
    [SerializeField] private float cohesionRadius = 10f;
    [SerializeField] private float alignmentWeight = 20f;
    [SerializeField] private float separationWeight = 100f;
    [SerializeField] private float separationMinRadius = 3f;

    private RunningCharacterController player;
    private List<Boid> inScene = new List<Boid>();             // List of all possible Boids in the scene. By default just idling

    public static BoidManager instance;

    private void Awake()
    {
        Singleton();
        DontDestroyOnLoad(gameObject);
    }

    private void Singleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        // Debug.Log("Done for " + this.name);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        SpawnBoid(new Vector3(30, 7, 30));
        SpawnBoid(new Vector3(24, 7, 24));
        SpawnBoid(new Vector3(22, 7, 22));
    }

    private void SpawnBoid(Vector3 pos)
    {
        Boid boid = Instantiate(boidPrefab, pos, Quaternion.identity, transform);
        SubscribeBoidEvents(boid);
        inScene.Add(boid);
    }

    private void SubscribeBoidEvents(Boid boid)
    {
        boid.detectedPlayer += StartFollow;
        boid.askSteering += CalculateBoidSteering;
    }

    private void StartFollow(Boid boid)
    {
        if (!player) return;
        // Maybe do something?
    }

    private void Separate(Boid boid)
    {
        // Removes boid from flock for purposes of shooting it off somewhere
    }

    private void CalculateBoidSteering(Boid boid)
    {
        Vector3 steerForce = Vector3.zero;
        steerForce += SteerToCenterForce(boid);
        steerForce = steerForce * steerForce.magnitude * cohesionWeight;
        steerForce += SteerSimilarVelocityForce(boid) * alignmentWeight;
        steerForce += RepulsiveForceTotal(boid) * separationWeight;
        steerForce = Vector3.ClampMagnitude(steerForce, player.GetRigidbody().GetAccumulatedForce().magnitude);
        boid.SetSteering(steerForce);
        // CLAMPPPPP
    }

    private Vector3 SteerToCenterForce(Boid boid)
    {
        Vector3 avgPos = Vector3.zero;
        int count = 0;
        foreach (Boid neigh in inScene)
        {
            float dist = Vector3.Distance(neigh.transform.position, boid.transform.position);
            if(dist > cohesionRadius || dist == 0) continue;
            // Dont count far away boids
            avgPos += neigh.transform.position;
            count++;
        }
        if (count == 0) return Vector3.zero;
        return avgPos/count - boid.transform.position;
    }

    private Vector3 SteerSimilarVelocityForce(Boid boid)
    {
        Vector3 avgVel = player.GetRigidbody().linearVelocity;
        int count = 1;
        foreach (Boid neigh in inScene)
        {
            float dist = Vector3.Distance(neigh.transform.position, boid.transform.position);
            if(dist > cohesionRadius || dist == 0) continue;
            // Dount count far away boids
            avgVel += neigh.GetVelocity();
            count++;
        }
        return Vector3.ClampMagnitude(avgVel/count, 1);
    }

    private Vector3 RepulsiveForceTotal(Boid boid)
    {
        float dist;
        Vector3 sepForce = Vector3.zero;
        int count = 0;
        foreach (Boid neigh in inScene)
        {
            dist = Vector3.Distance(neigh.transform.position, boid.transform.position);
            if(dist > separationMinRadius || dist == 0) continue;
            // Dount count far away boids
            sepForce += SepForce(boid.transform.position, neigh.transform.position, dist);
            count++;
        }

        dist = Vector3.Distance(player.transform.position, boid.transform.position);
        if(dist <= separationMinRadius)
        {
            sepForce += SepForce(player.transform.position, boid.transform.position, dist);
            count++;
        }

        if (count == 0) return Vector3.zero;
        return sepForce/count;
    }

    private Vector3 SepForce(Vector3 boidPos, Vector3 otherPos, float dist)
    {
        return (boidPos - otherPos).normalized / Mathf.Pow(dist, 3);
    }
}
