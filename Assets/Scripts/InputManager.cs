using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private RunningCharacterController player;

    public event Action<Vector2> HandleRun;
    public event Action<Vector2> HandleLook;
    public event Action<bool> HandleDescend;
    public event Action HandleJump;

    public static InputManager instance;

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
        player.SubscribeInputs();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        HandleRun?.Invoke(value);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        HandleLook?.Invoke(value);
    }

    public void OnDescend(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            HandleDescend?.Invoke(true);
        }
        else if(context.canceled)
        {
            HandleDescend?.Invoke(false);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            HandleJump?.Invoke();
        }
    }
}
