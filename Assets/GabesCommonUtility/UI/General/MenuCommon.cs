using UnityEngine;

namespace GabesCommonUtility.UI.General
{
    public class MenuCommon : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void OpenURL(string websiteUrl)
        {
            // If the websiteUrl is not empty, attempt to load the website
            if (!string.IsNullOrEmpty(websiteUrl))
            {
                Application.OpenURL(websiteUrl); // Open the website URL
            }
            else
            {
                Debug.LogError("Tried to load an empty string website", gameObject);
            }
        }

        public void CloseGame()
        {
            Application.Quit();
        }
    }
}