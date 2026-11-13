using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Element Links")]
    [SerializeField] private TMP_Text speedCounter;
    [SerializeField] private GameObject comboObject;
    [SerializeField] private TMP_Text scoreCounter;
    [SerializeField] private TMP_Text dragonCounter;

    private TMP_Text comboCounter;
    private Slider comboSlider;
    private uint targetScore = 0;
    private uint displayedScore = 0;

    [Header("Pop Up Prefabs")]
    [SerializeField] private List<GameObject> popUpPrefabs;
    [SerializeField] private float comboPopUpDisplayTime = 1f;
    [SerializeField] private GameObject scorePopUpPrefab;
    [SerializeField] private float scorePopUpDisplayTime = 3f;

    private bool counterCountdown = false;

    private RunningCharacterController player;
    private Rigidbody rb;
    private GameObject mainCanvas;
    
    public static UIManager instance;

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
        player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        rb = player.GetComponent<Rigidbody>();
        comboSlider = comboObject.GetComponentInChildren<Slider>(true);
        comboCounter = comboObject.GetComponentInChildren<TMP_Text>(true);
        SubscribePlayerEvents();
        SubscribeScoreEvents();
    }

    private void SubscribePlayerEvents()
    {
        if(player != null)
        {
            player.goodLanding += OnGoodLanding;
            player.comboTimerPause += OnComboPause;
            player.comboInterrupt += OnComboInterrupt;
        }
    }

    private void SubscribeScoreEvents()
    {
        ScoreManager.instance.scoreUpdated += UpdateCurrentScore;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeed();
    }

    private void FixedUpdate()
    {
        UpdateCombo();
        UpdateScoreDisplay();
    }

    private void OnGoodLanding(int comboCount, float comboWindow)
    {
        comboObject.SetActive(true);
        comboCounter.text = FormatCombo(comboCount);
        comboSlider.maxValue = comboWindow;
        comboSlider.value = comboWindow;
        counterCountdown = true;
        LandingPopUp();
        // Maybe also add a little screenshake
    }

    private void OnComboPause()
    {
        counterCountdown = false;
    }

    private void OnComboInterrupt()
    {
        comboObject.SetActive(false);
    }

    private void UpdateSpeed()
    {
        speedCounter.text = FormatSpeed(rb.linearVelocity.magnitude);
    }

    private void UpdateCombo()
    {
        if(comboObject.activeInHierarchy == true && counterCountdown)
        {
            comboSlider.value -= Time.fixedDeltaTime;
        }
    }

    private void UpdateScoreDisplay()
    {
        if(displayedScore < targetScore)
        {
            displayedScore = (uint)Mathf.Clamp(displayedScore + Mathf.Ceil((targetScore - displayedScore) / 10f), 0, targetScore);
            scoreCounter.text = displayedScore.ToString();
        }
    }

    public void UpdateDragonsCount(uint newCount)
    {
        dragonCounter.text = FormatDragonDisplay(newCount);
    }

    private void LandingPopUp()
    {
        int i = Random.Range(0, 3);
        GameObject popUp = GameObject.Instantiate(popUpPrefabs[i], Vector3.zero, Quaternion.identity, mainCanvas.transform);
        popUp.transform.localPosition = GetRandomCoords();
        StartCoroutine(PopUpDestroyCoroutine(popUp, comboPopUpDisplayTime));
    }

    private Vector3 GetRandomCoords()
    {
        return new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, -30f), 0);
    }

    private IEnumerator PopUpDestroyCoroutine(GameObject popUp, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(popUp);
    }

    private void UpdateCurrentScore(uint newScore)
    {
        ScorePopup(newScore - targetScore);
        targetScore = newScore;
    }
    
    private void ScorePopup(uint scoreGain)
    {
        GameObject popUp = Instantiate(scorePopUpPrefab, Vector3.zero, Quaternion.identity, scoreCounter.transform);
        popUp.transform.localPosition = GetRandomCoords();
        TMP_Text textBox;
        if(popUp.TryGetComponent<TMP_Text>(out textBox))
        {
            textBox.text = FormatScorePopup(scoreGain);
        }
        Rigidbody2D rb2d;
        if(popUp.TryGetComponent<Rigidbody2D>(out rb2d))
        {
            rb2d.AddForce(UnityEngine.Random.insideUnitCircle * 500, ForceMode2D.Impulse);
        }
        StartCoroutine(PopUpDestroyCoroutine(popUp, scorePopUpDisplayTime));
    }

    private string FormatSpeed(float speed)
    {
        float disp = Mathf.Round(speed * 10f) * 0.1f;
        return disp.ToString("000.0") + " m/s";
    }

    private string FormatCombo(int comboCount)
    {
        return "x" + comboCount.ToString("00");
    }

    private string FormatScorePopup(uint score)
    {
        return "+" + score.ToString();
    }

    private string FormatDragonDisplay(uint num)
    {
        return "x" + num.ToString();
    }
}
