using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class HumanPlayer : Human, IControllable
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _sprintAction;
        
        private Vector2 _moveInput;
        private bool _isSprinting;
        private bool _isEnabled;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Awake()
        {
            // Player-specific initialization if needed
        }

        public override float Speed => _moveInput.sqrMagnitude;

        public override void OnControllerEnabled(HumanController controller)
        {
            _controller = controller;

            _isEnabled = true;
        }

        public override void OnControllerDisabled()
        {
            _isEnabled = false;
            
            // Clean up input bindings
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMove;
                _moveAction.canceled -= OnMove;
            }
            
            if (_sprintAction != null)
            {
                _sprintAction.performed -= OnSprint;
                _sprintAction.canceled -= OnSprint;
            }

            _moveInput = Vector2.zero;
            _isSprinting = false;
            
        }

        public override void EndRoll()
        {
            Debug.Log("Roll Ended");
        }

        private void Update()
        {
            if (!_isEnabled) return;

            UpdateAnimation();
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                // Get camera-relative movement direction
                Vector3 moveDirection = GetCameraRelativeMovement();
                
                // Set agent speed based on sprint
                float currentSpeed = _isSprinting ? _controller.Stats.ChaseMoveSpeed : _controller.Stats.BaseMoveSpeed;
      
                
                // Rotate to face movement direction
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _controller.Stats.BaseMoveSpeed);
                }
            }
        }

        private Vector3 GetCameraRelativeMovement()
        {
            // Get camera forward and right vectors (flattened to horizontal plane)
            Transform cameraTransform = _camera.transform;
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            
            // Calculate movement direction relative to camera
            return (forward * _moveInput.y + right * _moveInput.x).normalized;
        }

        protected override void UpdateAnimation()
        {
            base.UpdateAnimation();
            
            // You can add player-specific animations here
            // For example, different animations for sprinting
            if (_isSprinting && _moveInput.sqrMagnitude > 0.01f)
            {
                // Animator.SetBool("IsSprinting", true);
            }
        }

        #region Input Callbacks

        public void OnControlsGained(PlayerInput input)
        {
            _playerInput = input;
            
            // Bind movement actions
            _moveAction = _playerInput.actions["Move"];
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
            
            // Bind sprint action (if you have one)
            _sprintAction = _playerInput.actions["Sprint"];
            if (_sprintAction != null)
            {
                _sprintAction.performed += OnSprint;
                _sprintAction.canceled += OnSprint;
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.performed;
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up to prevent memory leaks
            OnControllerDisabled();
        }
    }
}