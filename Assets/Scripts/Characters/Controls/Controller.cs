using UnityEngine;

public static class Controller
{
    private static Inputs inputs;
    private static IControllable controllable;

    private static void Initialize()
    {
        inputs = new Inputs();
        inputs.Player.Move.performed += context => controllable.Move(context.ReadValue<Vector2>());
        inputs.Player.Look.performed += context => controllable.Look(context.ReadValue<Vector2>());
        inputs.Player.Jump.performed += context => controllable.Jump();
        inputs.Player.Collect.performed += context => controllable.Collect();
        inputs.Player.Grenade.performed += context => controllable.ThrowGrenadeInput(); // Add this line
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public static void BindController(IControllable current)
    {
        controllable = current;
        if (inputs == null) Initialize();
        inputs!.Enable();
    }
}