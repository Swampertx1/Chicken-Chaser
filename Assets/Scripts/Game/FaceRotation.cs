using System;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class FaceRotation : NetworkBehaviour
    {
        private Rigidbody rb;
        
        [Header("Spin Settings")]
        [SerializeField] private float spinSpeed = 720f; // Degrees per second
        private float _currentSpinAngle = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (rb.linearVelocity.sqrMagnitude < 0.1f)
            {
                enabled = false;
                rb.constraints = RigidbodyConstraints.None;

                return;
            }

            Quaternion lookRotation = Quaternion.LookRotation(rb.linearVelocity);
            _currentSpinAngle += spinSpeed * Time.fixedDeltaTime;
            Quaternion spinRotation = Quaternion.Euler(0, 0, _currentSpinAngle);
            transform.rotation = lookRotation * spinRotation;
        }

        public override void OnNetworkSpawn()
        {
            enabled = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}