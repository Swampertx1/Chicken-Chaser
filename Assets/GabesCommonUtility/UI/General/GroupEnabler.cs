using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.General
{
    public class GroupEnabler : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private float delay;
        [SerializeField] private bool delayFirst;
    
        [SerializeField] private UnityEvent onNewItem;

        
        // Start is called before the first frame update
        void OnEnable()
        {
            StartCoroutine(Activate());
        }

        private IEnumerator Activate()
        {
            //QOL
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            if(delayFirst) yield return new WaitForSeconds(delay);
            for (int i = 0; i < transform.childCount; ++i)
            {
                onNewItem?.Invoke();
                transform.GetChild(i).gameObject.SetActive(true);
                yield return new WaitForSeconds(delay);
            }
            if(startButton) startButton.Select();
        }
    }
}
