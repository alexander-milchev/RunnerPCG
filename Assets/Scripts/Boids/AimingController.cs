using UnityEngine;

public class AimingController : MonoBehaviour
{
    private bool isAiming = false;
    private float timeScale = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SubscribeInputs();
    }

    private void SubscribeInputs()
    {
        InputManager.instance.HandleAim += StartAim;
        InputManager.instance.HandleShoot += Shoot;
        InputManager.instance.CancelAim += StopAim;
    }

    private void StartAim()
    {
        isAiming = true;
        timeScale = 0.05f;
        VFXManager.instance.SetDefaultTimeScale(timeScale);
        Time.timeScale = timeScale;
    }

    private void StopAim()
    {
        isAiming = false;
        timeScale = 1f;
        VFXManager.instance.SetDefaultTimeScale(timeScale);
        Time.timeScale = timeScale;
    }

    private void Shoot()
    {
        if(!isAiming) return;
        // get look direction and current position
        // move a boid to position, then make it shoot in given direction.
        StopAim();
    }
}
