using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GabesCommonUtility.UI.Select
{
    public class SelectableAction : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public UnityEvent onSelect;
        public UnityEvent onDeselect;
        
        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect?.Invoke();
        }
    }
}