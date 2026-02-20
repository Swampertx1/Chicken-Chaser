using Characters;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Game
{
    public class CaptureZone : NetworkBehaviour
    {
        [SerializeField] private ChickenTrapLander trapPrefab; // Changed from ThrowZone — see note below
        [SerializeField] private Transform chickenPoint;
        [SerializeField] private float throwForce;

        private bool _isPendingCapture;
        private Collider _collider;
        private Human _human;
        private Animator _animator;
        private ITrappable _caught;
        private ulong _caughtNetworkId;

        private void Awake()
        {
            _human = GetComponentInParent<Human>();
            _animator = GetComponentInParent<Animator>();
            _collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Trigger detection should only matter on the server
            if (!IsServer) return;

            if (other.attachedRigidbody.TryGetComponent(out _caught) && _caught.CanBeTrapped())
            {
                // Cache the network ID so we can pass it to the lander
                _caughtNetworkId = _caught.GetTransform()
                    .GetComponent<NetworkObject>().NetworkObjectId;

                _caught.OnPreCapture();

          /*      Transform tr = _caught.GetTransform();
                tr.SetParent(chickenPoint, true);
                tr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);  */

                // Trigger the animation on all clients
                TriggerCaptureAnimClientRpc();
                enabled = false;
            }
        }

        [ClientRpc]
        private void TriggerCaptureAnimClientRpc()
        {
            _animator.SetTrigger(StaticUtilities.BeginCaptureAnimID);
        }

        private void OnEnable()
        {
            _collider.enabled = true;
        }

        private void OnDisable()
        {
            _human.EndRoll();
            _collider.enabled = false;
        }

        /// <summary>
        /// Called by animation event. Must only fire on the server, or guard with IsServer.
        /// </summary>
        public void ThrowCaptureObject()
        {
            ThrowFromServer_Rpc();
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ThrowFromServer_Rpc()
        {
            GameObject trapGO = Instantiate(trapPrefab.gameObject, chickenPoint.position, Quaternion.identity);
            NetworkObject netObj = trapGO.GetComponent<NetworkObject>();
            netObj.Spawn();

            ChickenTrapLander lander = trapGO.GetComponent<ChickenTrapLander>();
            lander.Initialize(transform.forward * throwForce, _caught, _caughtNetworkId);
        }
    }
}