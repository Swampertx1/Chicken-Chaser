#if VIVOX
using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.Vivox
{
    public class VivoxSignOutSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;

        [SerializeField] private float timeUntilTimeOut = 90;
        
        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("VivoxInitializationSequence ... Waiting for netcode sign in...");
            float time = Time.time + timeUntilTimeOut;
            await UniTask.WhenAny(UniTask.WaitUntil(IsSignedIn), UniTask.WaitForSeconds(timeUntilTimeOut));
            if (time >= Time.time)
            {
                Debug.LogError("Vivox timed out...", gameObject);
                return failure as IEntrySequence;
            }

            await VivoxService.Instance.InitializeAsync();
            return Default;
        }

        private bool IsSignedIn()
        {
            return AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
        }

        public IEntrySequence Default => success as IEntrySequence;
        public bool IsCompleted => VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn;

        private void OnDrawGizmos()
        {
            if (success && success is not IEntrySequence)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }

            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("failure is INVALID", gameObject);
            }
        }
    }
}
#endif
