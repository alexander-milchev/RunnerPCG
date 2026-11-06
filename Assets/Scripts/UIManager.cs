using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Element Links")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text speedCounter;
    [SerializeField] private GameObject comboObject;

    private TMP_Text comboCounter;
    private Slider comboSlider;

    [Header("Pop Up Prefabs")]
    [SerializeField] private List<GameObject> popUpPrefabs;
    [SerializeField] private float displayTime = 1f;

    private bool counterCountdown = false;

    private RunningCharacterController player;
    private Rigidbody rb;
    
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
        rb = player.GetComponent<Rigidbody>();
        comboSlider = comboObject.GetComponentInChildren<Slider>(true);
        comboCounter = comboObject.GetComponentInChildren<TMP_Text>(true);
        SubscribePlayerEvents();
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

    // Update is called once per frame
    void Update()
    {
        UpdateSpeed();    
    }

    private void FixedUpdate()
    {
        UpdateCombo();
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

    private void LandingPopUp()
    {
        int i = Random.Range(0, 3);
        GameObject popUp = GameObject.Instantiate(popUpPrefabs[i], Vector3.zero, Quaternion.identity,
                                                GameObject.FindGameObjectWithTag("MainCanvas").transform);
        popUp.transform.localPosition = GetRandomCoords();
        StartCoroutine(PopUpDestroyCoroutine(popUp));
    }

    private Vector3 GetRandomCoords()
    {
        return new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, -30f), 0);
    }

    private IEnumerator PopUpDestroyCoroutine(GameObject popUp)
    {
        yield return new WaitForSeconds(displayTime);
        Destroy(popUp);
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
}
