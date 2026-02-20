using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class HumanPlayer : Human
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _sprintAction;
        private InputAction _lookAction;
        private InputAction _rollAction;

        private Vector3 _currentMoveDirection;
        private bool _isSprinting;

        [SerializeField] private float maxSpeed = 6;
        [SerializeField] private float maxSprintSpeed = 8;
        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private float sprintMoveSpeed = 6;

        private float currentMoveSpeed => _isSprinting ? sprintMoveSpeed : moveSpeed;
        private float currentMaxSpeed => _isSprinting ? maxSprintSpeed : maxSpeed;

        [SerializeField] private CinemachineCamera cam;
        [SerializeField] private Transform head;
        [SerializeField] private Transform body;
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField, Range(0, 89.9f)] private float maxPitch = 80f;

        private Vector2 _lookInput;

        public override float Speed => new Vector2(Rigidbody.linearVelocity.x, Rigidbody.linearVelocity.z).magnitude;

        public override void OnControllerEnabled(HumanController controller)
        {
            if (!IsOwner) return;
            
            enabled = true;
            this.controller = controller;

            cam.enabled = true;
            Rigidbody.isKinematic = false;
        }

        public override void OnControllerDisabled()
        {
            if (!IsOwner) return;

            
            enabled = false;
            cam.enabled = false;
            Rigidbody.isKinematic = true;

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

            if (_lookAction != null)
            {
                _lookAction.performed -= OnLook;
                _lookAction.canceled -= OnLook;
            }

            if (_rollAction != null)
            {
                _rollAction.performed -= OnRoll;
            }

            _currentMoveDirection = Vector3.zero;
            _isSprinting = false;

            StopRoll();
        }

        public override void EndRoll()
        {
            Debug.Log("Player roll ended");
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void LateUpdate()
        {
            UpdateAnimation();
            Look();
        }

        private void HandleMovement()
        {
            if (IsRolling) return;

            Rigidbody.AddForce(body.rotation * _currentMoveDirection * currentMoveSpeed, ForceMode.VelocityChange);
            Vector3 velocity = Rigidbody.linearVelocity;
            velocity.y = 0;
            float currentSpeed = velocity.magnitude;

            float max = currentMaxSpeed;
            if (currentSpeed > max)
            {
                Vector3 direction = velocity / currentSpeed;
                Rigidbody.linearVelocity = new Vector3(direction.x * max, Rigidbody.linearVelocity.y, direction.z * max);
            }
        }

        private void Look()
        {
            Vector2 direction = _lookInput * mouseSensitivity;
            body.Rotate(Vector3.up, direction.x);

            float pitch = head.localEulerAngles.x + direction.y;

            if (pitch > maxPitch && pitch < 180)
                pitch = maxPitch;
            else if (pitch < 360 - maxPitch && pitch > 180)
                pitch = 360 - maxPitch;

            head.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        protected override void UpdateAnimation()
        {
            base.UpdateAnimation();

            if (_isSprinting && _currentMoveDirection.sqrMagnitude > 0.01f)
            {
                // Animator.SetBool("IsSprinting", true);
            }
        }

        #region Input Callbacks

        public void OnControlsGained(PlayerInput input)
        {
            if (!IsOwner) return;

            _playerInput = input;

            _moveAction = _playerInput.actions["Move"];
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;

            _sprintAction = _playerInput.actions["Sprint"];
            _sprintAction.performed += OnSprint;
            _sprintAction.canceled += OnSprint;

            _lookAction = _playerInput.actions["Look"];
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;

            _rollAction = _playerInput.actions["Roll"];
            _rollAction.performed += OnRoll;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            _currentMoveDirection = new Vector3(input.x, 0, input.y);
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        private void OnRoll(InputAction.CallbackContext context)
        {
            Vector3 rollDirection = _currentMoveDirection.sqrMagnitude > 0.01f
                ? body.rotation * _currentMoveDirection
                : body.forward;

            TryRoll_Rpc(rollDirection.normalized);
        }

        #endregion

        private void OnDestroy()
        {
            OnControllerDisabled();
        }
    }
}