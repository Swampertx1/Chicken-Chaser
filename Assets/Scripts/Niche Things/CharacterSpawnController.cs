using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSpawnController : NetworkBehaviour
{
    [SerializeField] InputActionAsset playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!NetworkManager.IsConnectedClient)
        {
            giveControls();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            giveControls();
        }
    }

    public void giveControls()
    {
        
     var pi =   gameObject.AddComponent<PlayerInput>();
     pi.actions = playerInput;
        gameObject.AddComponent<Controller>();
        Destroy(this);
        Debug.Log("controls are gained", gameObject);
    }
  
}
