using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class HumanController : MonoBehaviour, IControllable
    {
        public Animator Animator { get; private set; }
        public AudioSource AudioSource { get; private set; }
    
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
            _currentController = GetComponent<IHuman>();
            
        }

        private void Start()
        {
            SwitchToAI();
        }


        public bool SwitchToAI()
        {
            if (!humanAIModule)
            {
                Debug.LogError("NO AI MODULE", gameObject);
                return false;
            }

            if (humanPlayerModule)
            {
                humanPlayerModule.OnControllerDisabled();
            }
            humanAIModule.OnControllerEnabled(this);
            _currentController = humanAIModule;
            return true;
        }
    
        public bool SwitchToPlayerControl()
        {
            if (!humanPlayerModule)
            {
                Debug.LogError("NO PLAYER MODULE", gameObject);
                return false;
            }

            if (humanAIModule)
            {
                humanAIModule.OnControllerDisabled();
            }
            humanPlayerModule.OnControllerEnabled(this);
            _currentController = humanPlayerModule;
            return true;
        }

        public void OnControlsGained(PlayerInput input)
        {
            
        }
    }
}