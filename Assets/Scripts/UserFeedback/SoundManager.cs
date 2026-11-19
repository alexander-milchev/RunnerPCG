using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip TITLE_MUSIC_LOOP;
    [SerializeField] private AudioClip IN_GAME_LOOP;

    private AudioSource audioSource;

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
        audioSource.Play();
        Debug.Log(audioSource.isPlaying);
    }

    private void StartGameMusic()
    {
        GetMusicSource();
        audioSource.clip = IN_GAME_LOOP;
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log(audioSource.isPlaying);
    }

    private void GetMusicSource()
    {
        audioSource = GameObject.FindWithTag("MainCamera").GetComponentInChildren<AudioSource>();
        Debug.Log(audioSource.name);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
