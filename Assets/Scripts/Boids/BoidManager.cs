using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    private List<Boid> followingPlayer = new List<Boid>();      // List of specifically Boids following player

    public static BoidManager instance;

    private void Awake()
    {
        Singleton();
    }

    private void Singleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        SpawnBoid(new Vector3(30, 7, 30));
        SpawnBoid(new Vector3(24, 7, 24));
        SpawnBoid(new Vector3(22, 7, 22));
    }

    public void SpawnBoid(Vector3 pos)
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

    private void UnsubscribeBoidEvents(Boid boid)
    {
        boid.detectedPlayer -= StartFollow;
        boid.askSteering -= CalculateBoidSteering;
    }

    private void StartFollow(Boid boid)
    {
        if (!player) return;
        followingPlayer.Add(boid);
        UIManager.instance.UpdateDragonsCount((uint)followingPlayer.Count);
    }

    private void Separate(Boid boid)
    {
        if (!player) return;
        followingPlayer.Remove(boid);
        UIManager.instance.UpdateDragonsCount((uint)followingPlayer.Count);
    }

    public void DespawnBoid(Boid boid, bool playParticles)
    {
        inScene.Remove(boid);
        if(playParticles) VFXManager.instance.SpawnDestroyParticles(boid.transform, 2f);
        UnsubscribeBoidEvents(boid);
        Destroy(boid.gameObject);
    }

    public void BoidSavePlayer(Boid boid)
    {           // Guaranteed to be in scene and following player.
        if(!player) return;
        Separate(boid);
        DespawnBoid(boid, true);
        player.GotSaved();
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

    public Boid GetBoidFollowingPlayer()
    {
        if(followingPlayer.Count > 0) return followingPlayer[0];
        else return null;
    }

    public IEnumerator ShootBoid(Boid boid, Vector3 pos, Vector3 goal, bool shootAt)
    {
        Separate(boid);
        yield return null;
        StartCoroutine(boid.ShootSelfFromTo(pos, goal, shootAt));
        // move a boid to position, then make it shoot in given direction.
    }

    public bool GetIsPlayerInvuln()
    {
        if(!player) return false;
        return player.GetIsInvuln();
    }
}
