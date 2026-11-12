using System;
using UnityEngine;

public abstract class DestructibleObject : MonoBehaviour
{
    [SerializeField] private int destroyScore;
    public virtual void Start()
    {
        
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(collision.impulse.magnitude > 4f)
            {
                Explode(collision.transform);
                Vector3 impulse = collision.impulse * -1;
                collision.rigidbody.AddForce(impulse, ForceMode.Impulse);
            }
        }
    }

    public virtual void Explode(Transform transform)
    {
        VFXManager.instance.SpawnDestroyParticles(transform);
        Destroy(this.gameObject);
    }
}
