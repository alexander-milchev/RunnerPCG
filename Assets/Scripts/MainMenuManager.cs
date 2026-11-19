using TMPro;
using Unity.Animations.SpringBones.GameObjectExtensions;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text HIGHSCORE_FIELD;
    [SerializeField] private GameObject MAIN_SCREEN;
    [SerializeField] private GameObject HOW_TO_PLAY;
    [SerializeField] private GameObject SETTINGS;
    private Slider soundSlider;

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
        UpdateHighestScore();
        soundSlider = SETTINGS.FindChildByName("Sound").GetComponentInChildren<Slider>();
        soundSlider.value = SoundManager.instance.GetSoundLevel();
    }

    private void UpdateHighestScore()
    {
        HIGHSCORE_FIELD.text = FormatHighScore(GameManager.instance.GetHighscore());
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

    public void OnSoundChanged()
    {
        SoundManager.instance.UpdateSoundLevel(soundSlider.value);
    }

    private string FormatHighScore(uint newScore)
    {
        return "Highest Score: " + newScore.ToString();
    }
}
