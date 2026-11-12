using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private uint score = 0;
    private uint landingMult = 0;
    private uint destroyMult = 1;

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
        player = GameObject.FindWithTag("Player").GetComponent<RunningCharacterController>();
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        player.goodLanding += IncrementCombo;
        player.comboInterrupt += ResetCombo;
    }

    private void IncrementCombo(int consecutive, float _)
    {
        landingMult = (uint)consecutive;
    }

    private void ResetCombo()
    {
        landingMult = 0;
        destroyMult = 1;
    }

    public void AddScore(uint added)
    {
        score = (uint)(score + added * (destroyMult + landingMult));
        scoreUpdated.Invoke(score);
        if(landingMult != 0)
        {
            destroyMult++;
        }
    }

    public uint GetScore()
    {
        return score;
    }
}
