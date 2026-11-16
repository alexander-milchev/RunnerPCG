using System;
using UnityEngine;

public abstract class DestructibleObject : MonoBehaviour
{
    [SerializeField] private uint destroyScore;
    public virtual void Start(){}

    public virtual void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(collision.impulse.magnitude > 4f)
            {           // If above set threshold, it's a hard collision, not a near hit/graze/tap
                HandlePlayerCollision(collision);
            }
        }
    }

    public virtual void HandlePlayerCollision(Collision collision)
    {
        Explode(collision.transform);
        Vector3 impulse = collision.impulse * -1;
        collision.rigidbody.AddForce(impulse, ForceMode.Impulse);
    }

    public virtual void Explode(Transform transform)
    {
        VFXManager.instance.SpawnDestroyParticles(transform, 0.3f);
        ScoreManager.instance.AddScore(destroyScore);
        Destroy(this.gameObject);
    }
}
