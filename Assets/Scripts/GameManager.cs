using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private uint highScore = 0;

    public event Action<int> sceneChanged;

    public static GameManager instance;

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        ChangeToScene(1);
        SceneManager.sceneLoaded += SceneLoadingFinished;
    }

    public void EndGame()
    {
        if (highScore < ScoreManager.instance.GetScore())
        {
            highScore = ScoreManager.instance.GetScore();
        }
        ChangeToScene(0);
    }

    private void ChangeToScene(int newID)
    {
        SceneManager.LoadScene(newID);
    }

    private void SceneLoadingFinished(Scene scene, LoadSceneMode mode)
    {
        sceneChanged?.Invoke(scene.buildIndex);
    }

    public uint GetHighscore()
    {
        return highScore;
    }
}
