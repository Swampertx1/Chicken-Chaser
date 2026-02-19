using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [DefaultExecutionOrder(100)]
    public class HumanController : MonoBehaviour, IControllable
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

        private void Start()
        {
            if (IsPlayerControlled) return;
            SwitchToAI();
        }


        public bool SwitchToAI()
        {        
            Debug.Log("Switching to AI PLAYER");

            
            if (!humanAIModule)
            {
                Debug.LogError("NO AI MODULE", gameObject);
                return false;
            }

            if (humanPlayerModule)
            {
                humanPlayerModule.OnControllerDisabled();
                humanPlayerModule.enabled = false;

            }
            
            humanAIModule.OnControllerEnabled(this);
            humanAIModule.enabled = true;

            _currentController = humanAIModule;
            return true;
        }
    
        public void SwitchToPlayerControl()
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
            SwitchToPlayerControl();
            humanPlayerModule.OnControlsGained(input);
            
        }
    }
}