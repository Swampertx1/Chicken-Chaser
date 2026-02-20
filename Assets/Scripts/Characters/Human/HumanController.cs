using Game;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [DefaultExecutionOrder(100)]
    public class HumanController : NetworkBehaviour, IControllable
    {
        public Animator Animator { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
    
        [SerializeField] private AiStats stats;
        
        public AiStats Stats => stats;
    
        private IHuman _currentController;

        [Header("OptionalModules")]
        [SerializeField] private HumanAI humanAIModule;
        [SerializeField] private HumanPlayer humanPlayerModule;
        
        public bool IsPlayerControlled => _currentController is HumanPlayer;
        public IHuman CurrentController => _currentController;
        
        
        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            AudioSource = GetComponentInChildren<AudioSource>();
            Rigidbody = GetComponentInChildren<Rigidbody>();
            _currentController = GetComponent<IHuman>();
            
        }

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.IsServer) 
                SwitchToAI_Rpc();
        }


        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void SwitchToAI_Rpc()
        {        
            Debug.Log("Switching to AI PLAYER");

            
            if (!humanAIModule)
            {
                Debug.LogError("NO AI MODULE", gameObject);
                return;
            }

            if (humanPlayerModule)
            {
                humanPlayerModule.OnControllerDisabled();
                humanPlayerModule.enabled = false;

            }
            
            humanAIModule.OnControllerEnabled(this);
            humanAIModule.enabled = true;

            _currentController = humanAIModule;
        }
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Owner)]
        public void SwitchToPlayerControl_Rpc()
        {
            Debug.Log("Switching to HUMAN PLAYER");

            if (!humanPlayerModule)
            {
                Debug.LogError("NO PLAYER MODULE", gameObject);
                return;
            }

            if (humanAIModule)
            {
                humanAIModule.OnControllerDisabled();
                humanAIModule.enabled = false;
            }
            humanPlayerModule.OnControllerEnabled(this);
            humanPlayerModule.enabled = true;
            _currentController = humanPlayerModule;
        }

        public void OnControlsGained(PlayerInput input)
        {
            SwitchToPlayerControl_Rpc();
            humanPlayerModule.OnControlsGained(input);
        }
    }
}