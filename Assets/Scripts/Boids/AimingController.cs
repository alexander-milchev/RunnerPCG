using UnityEngine;

public class AimingController : MonoBehaviour
{
    protected Camera camera;
    private bool isAiming = false;
    private float timeScale = 1f;
    void Start()
    {
        SubscribeInputs();
        camera = this.GetComponentInChildren<Camera>();
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
        UIManager.instance.EnableCrosshair(true);
        Time.timeScale = timeScale;
    }

    private void StopAim()
    {
        isAiming = false;
        timeScale = 1f;
        VFXManager.instance.SetDefaultTimeScale(timeScale);
        UIManager.instance.EnableCrosshair(false);
        Time.timeScale = timeScale;
    }

    private void Shoot()
    {
        if(!isAiming) return;
        Boid boid = BoidManager.instance.GetBoidFollowingPlayer();
        if(boid != null)
        {
            Vector3 shootPos = transform.position;
            Vector3 shootGoal = camera.transform.forward;
            bool shootingAtPoint = false;
            RaycastHit aimAt;
            if(Physics.Raycast(transform.position, camera.transform.forward, out aimAt))
            {
                shootingAtPoint = true;
                shootGoal = aimAt.point;
            }
            StartCoroutine(BoidManager.instance.ShootBoid(boid, shootPos, shootGoal, shootingAtPoint));    
            if(shootingAtPoint) Debug.DrawRay(shootPos, shootGoal, Color.red, 200f);
        }

        StopAim();
    }
}
