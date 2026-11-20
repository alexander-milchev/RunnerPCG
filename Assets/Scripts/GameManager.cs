using System;
using System.Collections;
using Unity.Animations.SpringBones.GameObjectExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float deathScreenDuration = 5f;
    private uint highScore = 0;
    private RunningCharacterController player;

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

    public void StartGame()
    {
        ChangeToScene(1);
        SceneManager.sceneLoaded += SceneLoadingFinished;
    }

    public void EndGame()
    {
        player.Explode();
        StartCoroutine(DeathScreenDuration());
    }

    private void ChangeToScene(int newID)
    {
        SceneManager.LoadScene(newID);
    }

    private void SceneLoadingFinished(Scene scene, LoadSceneMode mode)
    {
        sceneChanged?.Invoke(scene.buildIndex);
        if(scene.buildIndex == 0)
        {
            player = null;
        }
        else if(scene.buildIndex == 1)
        {
            player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        }
    }

    private IEnumerator DeathScreenDuration()
    {
        UIManager.instance.EnableDeathScreen(true);
        // Maybe play death music?
        yield return new WaitForSeconds(deathScreenDuration);   
        if (highScore < ScoreManager.instance.GetScore())
        {
            highScore = ScoreManager.instance.GetScore();
        }
        ChangeToScene(0);
    }

    public uint GetHighscore()
    {
        return highScore;
    }
}
