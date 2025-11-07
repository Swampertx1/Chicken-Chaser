using UnityEngine;

namespace GabesCommonUtility.UI.General
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCameraAutoAttach : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }
}
