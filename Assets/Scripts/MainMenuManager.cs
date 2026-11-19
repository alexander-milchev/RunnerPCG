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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        GameManager.instance.StartGame();
    }

    public void HowToPlay()
    {
        MAIN_SCREEN.SetActive(false);
        HOW_TO_PLAY.SetActive(true);
    }

    public void Settings()
    {
        MAIN_SCREEN.SetActive(false);
        SETTINGS.SetActive(true);
    }

    public void Back()
    {
        MAIN_SCREEN.SetActive(true);
        HOW_TO_PLAY.SetActive(false);
        SETTINGS.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
