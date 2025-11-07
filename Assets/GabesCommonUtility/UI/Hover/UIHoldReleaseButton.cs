using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Hover
{
    public class UIHoldReleaseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private float transitionTime;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color disabledColor;
        
        private bool _interactable = true;
        private Coroutine _coroutine;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable == value) return;
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    _coroutine = null;
                } 
                _coroutine =StartCoroutine(ColorTransition(value ? defaultColor : disabledColor));
                _interactable = value;
            }
        }

        public bool IsPressed { get; private set; }

        public UnityEvent onBeginHold;
        public UnityEvent onEndHold;


        private void OnMouseDown()
        {
            Debug.Log("AHHH");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("POINTER DOWN");
            if (!Interactable) return;
            if(!IsPressed) onBeginHold.Invoke();
            IsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable)
            {
             //   Debug.Log("not interactable");
              //  return;
            }
            if(IsPressed) onEndHold.Invoke();
            IsPressed = false;
        }
        
        private IEnumerator ColorTransition(Color targetColor)
        {
            Color startColor = targetGraphic.color;
            float elapsedTime = 0f;

            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                targetGraphic.color = Color.Lerp(startColor, targetColor, elapsedTime / transitionTime);
                yield return null;
            }

            targetGraphic.color = targetColor;
            _coroutine = null;
        }
    }
}
