using UnityEngine;
using System;

public class Boid : MonoBehaviour
{
    private GameObject player;
    private SphereCollider detectionCollider;
    private Rigidbody rb;

    public event Action<Boid> detectedPlayer;
    public event Action<Boid> askSteering;

    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float obstacleAvoidanceRadius = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float distFromPlayer = 0f;
    private Vector3 steeringDirection = Vector3.zero;
    private Vector3 avoidanceForce = Vector3.zero;

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
        if(!player) return;
        FollowPlayer();
        ObstacleAvoidance();
        askSteering?.Invoke(this);
        ApplyForces();
    }

    private void FollowPlayer()
    {
        moveDirection = Vector3.Normalize(player.transform.position - this.transform.position);
        distFromPlayer = (player.transform.position - this.transform.position).magnitude;
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
        Vector3 finalForce = moveDirection * Mathf.Pow(distFromPlayer, 2) + steeringDirection;
        rb.AddForce(finalForce, ForceMode.Acceleration);
        rb.AddForce(avoidanceForce, ForceMode.Acceleration);
    }

    public void OnTriggerEnter(Collider col)
    {
        if(player) return;
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
}
