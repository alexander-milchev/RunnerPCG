using UnityEngine;
using System.Collections;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem boostParticles;
    [SerializeField] private ParticleSystem jumpParticles;

    private RunningCharacterController player;
    private Rigidbody rb;
    private Transform vFXSpawnPoint;

    public static VFXManager instance;

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
        rb = player.GetComponent<Rigidbody>();
        vFXSpawnPoint = player.transform.Find("VFXSpawnPoint");

        SubscribeVFXEvents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SubscribeVFXEvents()
    {
        if(player != null)
        {
            player.goodLanding += SpawnBoostParticles;
            player.jumpEvent += SpawnJumpParticles;
        }
    }

    private void SpawnBoostParticles(int comboCount, float comboWindow)
    {
        ParticleSystem ps = Instantiate(boostParticles, vFXSpawnPoint.position, vFXSpawnPoint.rotation);
        StartCoroutine(LandTimeStop());
    }

    private void SpawnJumpParticles()
    {
        ParticleSystem ps = Instantiate(jumpParticles, vFXSpawnPoint.position, vFXSpawnPoint.rotation);
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        ps.transform.rotation = ps.transform.rotation * rot;
    }

    private IEnumerator LandTimeStop()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 1f;
    }
}
