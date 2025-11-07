using UnityEngine;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Select
{
    public class EnableSelector : MonoBehaviour
    {
        [SerializeField] private Selectable startingSelectable;

        // Start is called before the first frame update
        private void OnEnable()
        {
            startingSelectable.Select();
        }
    }
}
