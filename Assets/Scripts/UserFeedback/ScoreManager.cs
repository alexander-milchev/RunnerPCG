using System;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private float scorePerSecondAirborne = 20f;
    private uint score = 0;
    private uint landingMult = 0;
    private uint destroyMult = 1;
    private bool airborne = false;
    private float accruedComboScore = 0f;

    private RunningCharacterController player;

    public event Action<uint> scoreUpdated;
    public static ScoreManager instance;
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

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if(airborne)
        {
            accruedComboScore += Time.deltaTime * scorePerSecondAirborne * Mathf.Pow(1.2f, landingMult);
            UIManager.instance.UpdateAccruedScoreDisplay((uint)Mathf.FloorToInt(accruedComboScore));
        }
    }

    public void PrepareStartGame()
    {
        score = 0;
        landingMult = 0;
        destroyMult = 1;
        airborne = false;
        accruedComboScore = 0f;

        player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        player.goodLanding += IncrementCombo;
        player.comboInterrupt += ResetCombo;
        player.comboTimerPause += AirborneCombo;
    }

    private void IncrementCombo(int consecutive, float _)
    {       // Called on good landing
        landingMult = (uint)consecutive;
        ResetAirborne();
    }

    private void ResetCombo()
    {
        ResetAirborne();
        landingMult = 0;
        destroyMult = 1;
    }

    private void ResetAirborne()
    {
        airborne = false;
        if(accruedComboScore > 0) AddScoreNoMults((uint)Mathf.FloorToInt(accruedComboScore));
        accruedComboScore = 0f;
    }

    private void AirborneCombo()
    {
        airborne = true;
    }

    public void AddScore(uint added)
    {
        added = (uint)(added * (destroyMult + landingMult));
        AddScoreNoMults(added);
        if(landingMult != 0)
        {
            destroyMult++;
        }
    }

    private void AddScoreNoMults(uint added)
    {
        score += added;
        scoreUpdated?.Invoke(score);
    }

    public uint GetScore()
    {
        return score;
    }
}
