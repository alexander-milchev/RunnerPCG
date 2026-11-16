using UnityEngine;

public class BoidDespawner : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        Boid boid;
        if(other.TryGetComponent<Boid>(out boid))
        {
            BoidManager.instance.DespawnBoid(boid, false);
        }
    }
}
