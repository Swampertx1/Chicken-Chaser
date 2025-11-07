#if VIVOX
using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Multiplayer.GameObjects;
using GabesCommonUtility.Sequence;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.Vivox
{
    public class VivoxSignInSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;

        [SerializeField] private float timeUntilTimeOut = 90;
        [SerializeField] private bool useDebugEcho;
        [SerializeField] private bool usePositionalAudio;
        [SerializeField] private ChatCapability chatCapability;
        
        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("VivoxInitializationSequence ... Waiting for netcode sign in...");
            await UniTask.WhenAny(UniTask.WaitUntil(IsSignedIn), UniTask.WaitForSeconds(timeUntilTimeOut));
            if (!IsSignedIn())
            {
                Debug.LogError("Vivox timed out...", gameObject);
                return failure as IEntrySequence;
            }

            try
            {
                await VivoxService.Instance.InitializeAsync();
                string username = AuthenticationService.Instance.PlayerName;
                if(string.IsNullOrEmpty(username)) username = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId)?"INVALID UESR NAME" : "ID: " + AuthenticationService.Instance.PlayerId;
                
                
                LoginOptions options = new LoginOptions
                {
                    DisplayName = username,
                    EnableTTS = true
                };

                if (string.IsNullOrEmpty(VivoxService.Instance.SignedInPlayerId))
                {
                    Debug.Log("Not already signed in to vivox, signing in...");
                    await VivoxService.Instance.LoginAsync(options);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Vivox failed to init: " + e, gameObject);
                return failure as IEntrySequence;
            }
            try
            {
            string code;
            #if UNITY_NETCODE_GAMEOBJECTS
                code = LobbySystem.Instance.LobbyCode();
                if (string.IsNullOrEmpty(code))
                {
                    Debug.LogError("We are not in a lobby, Could not join voice.", gameObject);
                    return failure as IEntrySequence;
                }
            #endif
                
            #if UNITY_EDITOR
            if (useDebugEcho)
            {
                await VivoxService.Instance.JoinEchoChannelAsync("DEBUG", chatCapability);
                return Default;
            }
            #endif

          
                if (usePositionalAudio)
                {
                    Debug.LogWarning("Using temporary vivox 3D default settings");
                    Channel3DProperties props = new Channel3DProperties();
                    await VivoxService.Instance.JoinPositionalChannelAsync("proximity", chatCapability, props,
                        new ChannelOptions());
                }
                else
                {
                    await VivoxService.Instance.JoinGroupChannelAsync(code, chatCapability, new ChannelOptions());
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Vivox failed to join lobby: " + e, gameObject);
                return failure as IEntrySequence;
            }

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

        private void OnApplicationQuit()
        {
            VivoxService.Instance.LogoutAsync();
        }
    }
}
#endif
