using UnityEngine;

public class SmallObject : DestructibleObject
{
    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
    }

    public override void HandlePlayerCollision(Collision collision)
    {
        base.HandlePlayerCollision(collision);
    }

    public override void Explode(Transform transform)
    {
        base.Explode(transform);
    }
}
