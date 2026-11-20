using UnityEngine;
using System.Collections;
using System;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem boostParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem destroyParticles;

    private RunningCharacterController player;
    private Transform vFXSpawnPoint;
    private GameObject invulnShield;
    private float defaultTimeScale = 1f;

    public event Action<Vector3> goodLandVFXSpawned;
    public event Action<Vector3> explodeVFXSpawned;
    public event Action<Vector3> jumpVFXSpawned;

    public static VFXManager instance;

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

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        vFXSpawnPoint = player.transform.Find("VFXSpawnPoint");
        invulnShield = player.transform.Find("ShieldSphere").gameObject;

        SubscribeVFXEvents();
        SubscribeSoundManager();
    }

    private void SubscribeVFXEvents()
    {
        if(player != null)
        {
            player.goodLanding += SpawnBoostParticles;
            player.jumpEvent += SpawnJumpParticles;
            player.toggleInvuln += ToggleInvulnParticles;
            player.onDeath += UnsubscribePlayerDestroyed;
        }
    }

    private void SubscribeSoundManager()
    {
        jumpVFXSpawned += SoundManager.instance.SpawnJumpSFX;
        explodeVFXSpawned += SoundManager.instance.SpawnExplodeSFX;
        goodLandVFXSpawned += SoundManager.instance.SpawnLandSFX;
    }

    private void UnsubscribePlayerDestroyed()
    {
        player.goodLanding -= SpawnBoostParticles;
        player.jumpEvent -= SpawnJumpParticles;
        player.toggleInvuln -= ToggleInvulnParticles;
        player.onDeath -= UnsubscribePlayerDestroyed;
    }

    private void UnsubscribeOnDestroy()
    {
        jumpVFXSpawned -= SoundManager.instance.SpawnJumpSFX;
        explodeVFXSpawned -= SoundManager.instance.SpawnExplodeSFX;
        goodLandVFXSpawned -= SoundManager.instance.SpawnLandSFX;
    }

    public void OnDestroy()
    {
        UnsubscribeOnDestroy();
    }

    private void SpawnBoostParticles(int comboCount, float comboWindow)
    {
        ParticleSystem ps = Instantiate(boostParticles, vFXSpawnPoint.position, vFXSpawnPoint.rotation);
        goodLandVFXSpawned?.Invoke(vFXSpawnPoint.position);
        StartCoroutine(LandTimeStop());
    }

    private void SpawnJumpParticles()
    {
        ParticleSystem ps = Instantiate(jumpParticles, vFXSpawnPoint.position, vFXSpawnPoint.rotation);
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        ps.transform.rotation = ps.transform.rotation * rot;
        jumpVFXSpawned?.Invoke(vFXSpawnPoint.position);
    }

    private IEnumerator LandTimeStop()
    {
        Time.timeScale = defaultTimeScale/5f;
        yield return new WaitForSeconds(defaultTimeScale/10f);
        Time.timeScale = defaultTimeScale;
    }

    public void SpawnDestroyParticles(Transform objectTransform, float scale)
    {
        ParticleSystem ps = Instantiate(destroyParticles, objectTransform.position, objectTransform.rotation);
        ps.transform.localScale = new Vector3(scale, scale, scale);
        explodeVFXSpawned?.Invoke(objectTransform.position);
    }

    public void SetDefaultTimeScale(float ts)
    {
        defaultTimeScale = ts;
    }

    private void ToggleInvulnParticles(bool toggle)
    {
        invulnShield.SetActive(toggle);
    }
}
