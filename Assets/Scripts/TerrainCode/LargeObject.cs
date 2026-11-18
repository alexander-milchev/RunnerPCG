using System;
using UnityEngine;

public class LargeObject : DestructibleObject
{
    [SerializeField] private GameObject destroyedPrefab;
    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
    }

    public override void HandlePlayerCollision(Collision collision)
    {
        if(BoidManager.instance.GetIsPlayerInvuln())
        {
            base.HandlePlayerCollision(collision);
            return;
        }
        Boid boid = BoidManager.instance.GetBoidFollowingPlayer();
        if(boid != null)
        {
            BoidManager.instance.BoidSavePlayer(boid);
            base.HandlePlayerCollision(collision);
        }
        else
        {
            // Player dies
            Debug.Log("death");
        }
    }

    public override void Explode(Transform transform)
    {
        GameObject ragdoll = Instantiate(destroyedPrefab, transform.position, transform.rotation, transform.parent);
        Rigidbody rb = ragdoll.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.AddForce(UnityEngine.Random.onUnitSphere * 10, ForceMode.Impulse);
        }
        base.Explode(transform);
    }
}
