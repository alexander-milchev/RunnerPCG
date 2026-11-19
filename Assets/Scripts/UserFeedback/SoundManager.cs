using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip TITLE_MUSIC_LOOP;
    [SerializeField] private AudioClip IN_GAME_LOOP;

    private AudioSource audioSource;
    private float preserveAudioVolume = 0.5f;

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
        audioSource.clip = TITLE_MUSIC_LOOP;
        audioSource.loop = true;
        audioSource.volume = preserveAudioVolume;
        audioSource.Play();
    }

    private void StartGameMusic()
    {
        GetMusicSource();
        audioSource.clip = IN_GAME_LOOP;
        audioSource.loop = true;
        audioSource.volume = preserveAudioVolume;
        audioSource.Play();
    }

    private void GetMusicSource()
    {
        audioSource = GameObject.FindWithTag("MainCamera").GetComponentInChildren<AudioSource>();
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

    public void UpdateSoundLevel(float newLevel)
    {
        preserveAudioVolume = newLevel;
        audioSource.volume = newLevel;
    }

    public float GetSoundLevel()
    {
        return preserveAudioVolume;
    }
}
