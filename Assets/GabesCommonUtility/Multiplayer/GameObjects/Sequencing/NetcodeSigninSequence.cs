using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
#if UNITY_EDITOR && PARREL_SYNC
using ParrelSync;
#endif

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class NetcodeSigninSequence : MonoBehaviour, IEntrySequence
    {
        public enum SignInMode
        {
            AnonymousEditor,
            Unity,
            Steam
        }

        [Header("Sign-In Settings")]
        [Tooltip("Auto: Chooses best sign-in for platform.\nAnonymous: Basic sign-in.\nUnity: Uses Unity ID.\nSteam: Uses Steamworks authentication.")]
        [SerializeField] private SignInMode signInMode = SignInMode.AnonymousEditor;

        [Tooltip("Optional: Next sequence to execute after sign-in.")]
        [SerializeField] private Behaviour next;
        [SerializeField] private Behaviour failure;
        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted =>  AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
        

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            try
            {
                await UnityServices.InitializeAsync();

                switch (signInMode)
                {
                    case SignInMode.AnonymousEditor:
                        await SignInAnonymousAsync();
                        break;

                    case SignInMode.Unity:
                        await SignInWithUnityAsync();
                        break;

                    case SignInMode.Steam:
                        await SignInWithSteamAsync();
                        break;
                }
            }
            catch (Exception e)
            {
                DisplayMessage?.Invoke("[[NetcodeSigninSequence]]: " + e.Message);   
                return failure as  IEntrySequence;
            }

            Debug.Log($"[NetcodeSigninSequence] Signed in successfully as {AuthenticationService.Instance.PlayerId}");
            return Default;
        }

#if PARREL_SYNC
        private string GetCloneNumber()
        {
            // Try to get custom argument from ParrelSync
            string argument = ClonesManager.GetArgument();
            if (!string.IsNullOrEmpty(argument))
            {
                return argument;
            }
            
            // If no custom argument, try to get project name which often contains clone number
            string projectName = ClonesManager.GetCurrentProjectPath();
            int cloneIndex = projectName.IndexOf("_clone_", StringComparison.Ordinal);
            if (cloneIndex != -1)
            {
                string afterClone = projectName.Substring(cloneIndex + 7);
                int endIndex = afterClone.IndexOfAny(new char[] { '/', '\\' });
                if (endIndex > 0)
                {
                    return afterClone.Substring(0, endIndex);
                }
            }
            return "INVALID";
        }
#endif

        private async UniTask SignInAnonymousAsync()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
#if UNITY_EDITOR && PARREL_SYNC
                // For ParrelSync clones, create a unique profile to avoid conflicts
                if ( ClonesManager.IsClone())
                {
                    Debug.Log("[NetcodeSigninSequence] ParrelSync clone detected - using Anonymous sign-in");
                    string cloneProfile = $"ParrelSync_Clone_{GetCloneNumber()}";
                    Debug.Log($"[NetcodeSigninSequence] Signing in with profile: {cloneProfile}");
                    AuthenticationService.Instance.SwitchProfile(cloneProfile);
                }
#endif
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                Debug.LogError("Somehow already signed in anonymously? Try Installing ParrelSync? https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync");
            }
        }

        private async UniTask SignInWithUnityAsync()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Replace with real Unity OAuth if needed
        }

        private async UniTask SignInWithSteamAsync()
        {
            Debug.LogError("This is not setup yet.");

            /*
#if UNITY_STANDALONE
            if (AuthenticationService.Instance.IsSignedIn)
                return;

            try
            {
                byte[] ticketData = new byte[1024];
                SteamNetworkingIdentity identity =
                SteamUser.GetAuthSessionTicket(ticketData, ticketData.Length, out uint ticketSize, );
                string ticket = System.Convert.ToBase64String(ticketData, 0, (int)ticketSize);

                await AuthenticationService.Instance.SignInWithSteamAsync();

                Debug.Log("[NetcodeSigninSequence] Steam sign-in successful.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NetcodeSigninSequence] Steam sign-in failed: {ex.Message}");
                await SignInAnonymousAsync(); // fallback
            }
#else
            Debug.LogWarning("[NetcodeSigninSequence] Steam sign-in attempted on a non-standalone platform.");
            await SignInAnonymousAsync();
#endif

        */
        }
        
        private void OnDrawGizmos()
        {
            if (next && Default == null)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("Failure is INVALID", gameObject);
            }
        }

    }
}