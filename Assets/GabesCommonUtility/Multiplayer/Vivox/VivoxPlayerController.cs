using System;

using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GabesCommonUtility.Multiplayer.Vivox
{
    public class VivoxPlayerController : MonoBehaviour
    {/*
        [SerializeField] private string channelName = "proximity";
        private bool isPositionTracking = false;

        private async void Start()
        {
            await InitializePositionalAudio();
        }

        private async UniTask InitializePositionalAudio()
        {
            try
            {
                // Wait for Vivox to be ready
                await UniTask.WaitUntil(() => 
                    VivoxService.Instance != null && 
                    VivoxService.Instance.IsLoggedIn
                );

                // Wait for the channel to be joined
                await UniTask.WaitUntil(() => 
                    VivoxService.Instance.ActiveChannels.Count > 0
                );

                // Now safe to set 3D position
                VivoxService.Instance.Set3DPosition(gameObject, channelName, true);
                isPositionTracking = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize positional audio: {e}", this);
            }
        }

        private void OnDestroy()
        {
            // Clean up position tracking
            if (isPositionTracking && VivoxService.Instance != null)
            {
                try
                {
                    VivoxService.Instance.Set3DPosition(gameObject, channelName, false);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to cleanup positional audio: {e.Message}");
                }
            }
        }*/
    }
}