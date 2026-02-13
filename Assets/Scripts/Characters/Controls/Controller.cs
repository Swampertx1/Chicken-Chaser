using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public  class Controller : MonoBehaviour
{
    private IControllable[] _controllables;
    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        Possess(gameObject);
    }

    public void Possess(GameObject target)
    {
        _controllables = GetComponentsInChildren<IControllable>();

        foreach (var controllable in _controllables)
        {
            controllable.OnControlsGained(playerInput);
            
        }
        playerInput.actions.Enable();
    }

    private void OnEnable()
    {
        playerInput ??= GetComponent<PlayerInput>();
        playerInput?.actions.Enable();
    }

    private void OnDisable()
    {
        playerInput ??= GetComponent<PlayerInput>();
        playerInput?.actions.Disable();
    }
    
  /////  private static Inputs inputs;
    /*private static IControllable controllable;

    private static void Initialize()
    {
        inputs = new Inputs();
        inputs.Player.Move.performed += context => controllable.Move(context.ReadValue<Vector2>());
        inputs.Player.Look.performed += context => controllable.Look(context.ReadValue<Vector2>());
        inputs.Player.Jump.performed += context => controllable.Jump();
        inputs.Player.Collect.performed += context => controllable.Collect();
        inputs.Player.Ability1.performed += context => controllable.ThrowGrenadeInput(); // Add this line
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public static void BindController(IControllable current)
    {
        controllable = current;
        if (inputs == null) Initialize();
        inputs!.Enable();
    } */
}