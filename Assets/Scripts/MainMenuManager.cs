using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MAIN_SCREEN;
    [SerializeField] private GameObject HOW_TO_PLAY;
    [SerializeField] private GameObject SETTINGS;

    public static MainMenuManager instance;

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
    }

    void Start()
    {
        Debug.Log(PlayerPrefs.GetString("WASD.Up"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        
    }

    public void HowToPlay()
    {
        
    }

    public void Settings()
    {
        
    }

    public void Back()
    {
        
    }

    public void Quit()
    {
        Application.Quit();
    }

}
