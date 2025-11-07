using UnityEngine;

namespace GabesCommonUtility.UI.Text
{

    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class TMPTextForceMaterial : MonoBehaviour
    {
        [SerializeField] private Material sharedMaterial;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            TMPro.TMP_Text text = GetComponent<TMPro.TMP_Text>();
            text.fontMaterial = sharedMaterial;
        }
    }
}