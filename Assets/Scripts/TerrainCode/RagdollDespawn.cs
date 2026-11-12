using UnityEngine;

public class RagdollDespawn : MonoBehaviour
{
    [SerializeField] private float secBeforeDespawn = 3f;
    private float startTime;
    void Start()
    {
        startTime = Time.time;
    }

    void FixedUpdate()
    {
        if(Time.time > startTime + secBeforeDespawn)
        {
            Destroy(gameObject);
        }
    }
}
