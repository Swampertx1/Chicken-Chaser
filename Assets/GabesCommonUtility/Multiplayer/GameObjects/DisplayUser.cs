#if UNITY_SERVICES
using System.Collections;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects
{
    public class DisplayUser : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public IEnumerator Start()
        {
            text ??=  GetComponent<TextMeshProUGUI>();
            yield return new WaitUntil(IsAuthenticationReady);
            string userName = AuthenticationService.Instance.PlayerName;
            userName = string.IsNullOrEmpty(userName)?"ID: " +AuthenticationService.Instance.PlayerId:userName;
            userName = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId)?"INVALID USER?":userName;
            text.text = userName;
        }

        private bool IsAuthenticationReady() => UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance != null && AuthenticationService.Instance.IsAuthorized;
    }
}
#endif