using UnityEngine;

public class AimingController : MonoBehaviour
{
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
        // reduce timescale to 0.01
    }

    private void StopAim()
    {
        // restore timescale to 1
    }

    private void Shoot()
    {
        // get look direction and current position
        // move a boid to position, then make it shoot in given direction.
        // restore timescale to 1
    }
}
