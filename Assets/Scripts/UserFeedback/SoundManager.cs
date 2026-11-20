using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Musics")]
    [SerializeField] private AudioClip TITLE_MUSIC_LOOP;
    [SerializeField] private AudioClip IN_GAME_LOOP;
    [Header("SFX")]
    [SerializeField] private AudioClip JUMP;
    [SerializeField] private AudioClip GOOD_LAND;
    [SerializeField] private AudioClip EXPLODE;

    private AudioSource musicSource;
    private float globalMusicVolume = 0.02f;
    private float globalSFXVolume = 0.1f;

    public static SoundManager instance;

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
        SubscribeSceneEvents();
        StartTitleMusic();
    }

    private void SubscribeSceneEvents()
    {
        GameManager.instance.sceneChanged += OnChangeScene;
    }

    private void OnDestroy()
    {
        GameManager.instance.sceneChanged -= OnChangeScene;
    }

    private void StartTitleMusic()
    {
        GetMusicSource();
        musicSource.clip = TITLE_MUSIC_LOOP;
        musicSource.loop = true;
        musicSource.volume = globalMusicVolume;
        musicSource.Play();
    }

    private void StartGameMusic()
    {
        GetMusicSource();
        musicSource.clip = IN_GAME_LOOP;
        musicSource.loop = true;
        musicSource.volume = globalMusicVolume;
        musicSource.Play();
    }

    private void GetMusicSource()
    {
        musicSource = GameObject.FindWithTag("MainCamera").GetComponentInChildren<AudioSource>();
    }

    private void OnChangeScene(int newScene)
    {
        switch(newScene)
        {
            case 0:
                StartTitleMusic();
                break;
            case 1:
                StartGameMusic();
                break;
            default:
                break;
        }
    }

    public void SpawnJumpSFX(Vector3 loc)
    {
        AudioSource.PlayClipAtPoint(JUMP, loc, globalSFXVolume);
    }

    public void SpawnLandSFX(Vector3 loc)
    {
        AudioSource.PlayClipAtPoint(GOOD_LAND, loc, globalSFXVolume*2);
    }

    public void SpawnExplodeSFX(Vector3 loc)
    {
        AudioSource.PlayClipAtPoint(EXPLODE, loc, globalSFXVolume);
    }

    public void UpdateMusicVolume(float newLevel)
    {
        globalMusicVolume = newLevel;
        if(musicSource == null) return;
        musicSource.volume = newLevel;
    }

    public void UpdateSFXVolume(float newLevel)
    {
        globalSFXVolume = newLevel;
    }

    public float GetMusicVolume()
    {
        return globalMusicVolume;
    }

    public float GetSFXVolume()
    {
        return globalSFXVolume;
    }
}
