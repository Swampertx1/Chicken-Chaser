#if UNITY_NETCODE_GAMEOBJECTS
using UnityEngine;

namespace GabesCommonUtility.Multiplayer
{
    /// <summary>
    /// Handle connections from both local and multiplayer 
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class CoreMultiplayerSystem : MonoBehaviour
    {
        public static CoreMultiplayerSystem Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.LogError("MultiplayerMainMenu is not implemented. this script is supposed to handle connection from both local and networked multiplayer.");
        }
    }
}
#endif