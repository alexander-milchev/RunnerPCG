using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int sceneID;        // 0 = Main menu, 1 = Game Scene

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

    void Start()
    {
        sceneID = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(SceneManager.sceneCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        ChangeToScene(1);
    }

    public void EndGame()
    {
        ChangeToScene(0);
    }

    private void ChangeToScene(int newID)
    {
        sceneID = newID;
        SceneManager.LoadScene(newID);
    }
}
