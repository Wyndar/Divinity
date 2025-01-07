using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position, float time);
    public event StartTouch OnEndTouch;

    private PlayerInputList playerInputs;

    private void Awake() => playerInputs = new PlayerInputList();

    private void OnEnable() => playerInputs.Enable();

    private void OnDisable() => playerInputs.Disable();

    private void Start()
    {
        playerInputs.Player.Tap.started += context => StartedTouch(context);
        playerInputs.Player.Tap.canceled += context => EndedTouch(context);
    }

    private void StartedTouch(InputAction.CallbackContext callbackContext) => OnStartTouch?.Invoke(playerInputs.Player.Position.ReadValue<Vector2>(), (float)callbackContext.startTime);

    private void EndedTouch(InputAction.CallbackContext callbackContext) => OnEndTouch?.Invoke(playerInputs.Player.Position.ReadValue<Vector2>(), (float)callbackContext.time);

   public Vector2 CurrentFingerPosition => playerInputs.Player.Position.ReadValue<Vector2>();
}



