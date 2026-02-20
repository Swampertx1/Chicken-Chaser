using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ChickenTrap : NetworkBehaviour
    {
        [SerializeField] private float decayTime = .8f;
        private NetworkVariable<float> _CurrentDecayTime = new();
        private ITrappable _trappable;
        private Material _myMaterial;
        private bool _isOpened;
        private void Awake()
        {
            if (transform.childCount > 0 && transform.GetChild(0).childCount > 0 &&
                transform.GetChild(0).GetChild(0).TryGetComponent(out _trappable))
            {
                PauseObject();
            }

            _myMaterial = GetComponent<MeshRenderer>().material;
        }

        private void OnTriggerStay(Collider other)
        {
            //When the chicken is freed, its triggering this again, and freeing itself twice because OnTriggerStay runs on the physics ticks
            if (_trappable == null  || !other.attachedRigidbody.TryGetComponent(out ITrappable c) || !c.CanBeTrapped() || _isOpened) return;
            _CurrentDecayTime.Value += Time.deltaTime * 2;
            _myMaterial.SetFloat(StaticUtilities.FillMatID, _CurrentDecayTime.Value / decayTime);
            if (_CurrentDecayTime.Value >= decayTime)
            {
                _isOpened = true;
                FreeChicken();
            }
        }

        private void LateUpdate()
        {
            if(_isOpened || _CurrentDecayTime.Value <= 0) return;
            _CurrentDecayTime.Value -= Time.deltaTime;
            if (_CurrentDecayTime.Value <= 0) _CurrentDecayTime.Value = 0;
            _myMaterial.SetFloat(StaticUtilities.FillMatID, _CurrentDecayTime.Value / decayTime);
        }

        private void FreeChicken()
        {
            _trappable.GetTransform().parent = null;
            _trappable.OnFreedFromCage();
            Destroy(gameObject);
        }

  

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void AttachChicken_Rpc(ulong id)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject obj);
            obj.TryGetComponent(out _trappable);
            PauseObject();
            _trappable.OnCaptured();
        }

        private void PauseObject()
        {
            Transform tr = _trappable.GetTransform();
            tr.parent = transform.GetChild(0);
            tr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _trappable.OnPreCapture(); //Disabling the AI component, SHOULD automatically enable the secondary look at component
        }
    }
}
