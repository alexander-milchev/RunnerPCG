using UnityEngine;
using System;
using System.Collections;

public class Boid : MonoBehaviour
{
    private GameObject player;
    private SphereCollider detectionCollider;
    private Rigidbody rb;

    public event Action<Boid> detectedPlayer;
    public event Action<Boid> askSteering;

    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float obstacleAvoidanceRadius = 3f;
    [SerializeField] private float shootingSpeed = 30f;
    [SerializeField] private float explodeRadius = 5f;

    private Vector3 moveDirection = Vector3.zero;
    private float distFromGoal = 0f;
    private Vector3 steeringDirection = Vector3.zero;
    private Vector3 avoidanceForce = Vector3.zero;
    private bool isBeingShot = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        detectionCollider = gameObject.AddComponent<SphereCollider>() as SphereCollider;
        detectionCollider.radius = detectionRadius;
        detectionCollider.isTrigger = true;
        detectionCollider.includeLayers = LayerMask.GetMask("Default");
        detectionCollider.layerOverridePriority = 999;
        detectionCollider.providesContacts = true;
        rb = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if(!player || isBeingShot) return;
        FollowPlayer();
        ObstacleAvoidance();
        askSteering?.Invoke(this);
        ApplyForces();
    }

    private void FollowPlayer()
    {
        FollowToPosition(player.transform.position);
    }

    private void FollowToPosition(Vector3 pos)
    {
        moveDirection = Vector3.Normalize(pos - this.transform.position);
        distFromGoal = (pos - this.transform.position).magnitude;
    }

    private void ObstacleAvoidance()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, obstacleAvoidanceRadius, Vector3.forward);
        
        Vector3 totalForce = Vector3.zero;
        int count = 0;
        foreach (RaycastHit hitInfo in hits)
        {
            if(hitInfo.distance <= 0 || hitInfo.transform.tag != "Ground") continue;
            totalForce += Vector3.ClampMagnitude(hitInfo.normal / Mathf.Pow(hitInfo.distance, 2), 500); 
            count++;
        }

        if(count == 0) 
        {
            avoidanceForce = Vector3.zero;
            return;
        }

        avoidanceForce = totalForce / count;
        //Debug.Log(avoidanceForce);
    }

    private void ApplyForces()
    {
        Vector3 finalForce = moveDirection * Mathf.Pow(distFromGoal, 2) + steeringDirection;
        rb.AddForce(finalForce, ForceMode.Acceleration);
        rb.AddForce(avoidanceForce, ForceMode.Acceleration);
    }

    public void OnTriggerEnter(Collider col)
    {
        if(player || isBeingShot) return;
        if(col.gameObject.tag == "Player")
        {
            detectedPlayer?.Invoke(this);
            player = col.gameObject;
        }
    }

    public Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public void SetSteering(Vector3 steer)
    {
        steeringDirection = steer;
    }

    public IEnumerator ShootSelfFromTo(Vector3 pos, Vector3 goal, bool shootAt)
    {
        float pVel = player.GetComponent<Rigidbody>().linearVelocity.magnitude;
        player = null;
        isBeingShot = true;
        avoidanceForce = Vector3.zero;
        steeringDirection = Vector3.zero;
        yield return null;

        rb.angularVelocity = Vector3.zero;
        gameObject.transform.position = pos;
        if(shootAt)
        {       // if shootAt is true, we were given a destination Vector3 in goal
            rb.linearVelocity = (goal - transform.position).normalized * (shootingSpeed + pVel);
        }
        else
        {       // if shootAt is false, we were given a direction in goal (camera.forward)
            rb.linearVelocity = goal * (shootingSpeed + pVel);
        }
        yield return null;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(!isBeingShot) return;

        ExplodeAndDestroy();
    }

    private void ExplodeAndDestroy()
    {
        Collider[] cols = UnityEngine.Physics.OverlapSphere(transform.position, explodeRadius);
        foreach(Collider col in cols)
        {
            DestructibleObject obj;
            if(col.transform.TryGetComponent<DestructibleObject>(out obj))
            {
                obj.Explode(obj.transform);
            }
        }
        BoidManager.instance.DespawnBoid(this, true);
    }
}
