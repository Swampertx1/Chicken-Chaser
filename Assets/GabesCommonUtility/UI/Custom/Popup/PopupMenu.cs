using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GabesCommonUtility.UI.Custom.Popup
{
   public class PopupMenu : MonoBehaviour
   {
      public UnityEvent onOpened;
      public UnityEvent onClosed;

   
      [Header("Transition")]
      [SerializeField] private RectTransform target;
      [SerializeField] private Vector2 upPosition;
      [SerializeField] private Vector2 downPosition;
      [SerializeField] private float transitionDuration = 0.5f;
      [SerializeField] private AnimationCurve transitionCurve;
      [SerializeField] private bool isVisible = true;

      private bool _isClosing;
      private bool _isOpening;
      
      private float _currentTransitionTime = 0;
      private Coroutine _actionRoutine;
      
      
      [ContextMenu("Toggle")]
      public void Toggle()
      {
         if(isVisible) Close();
         else Open();
      }

      public void SetState(bool state)
      {
         if(!state) Close();
         else Open();
      }

      public void Open()
      {
         if (_isOpening) return;
         _isClosing = false;
         _isOpening = true;
         if (_actionRoutine != null)
         {
            _currentTransitionTime = transitionDuration - _currentTransitionTime;
            StopCoroutine(_actionRoutine);
         }
         _actionRoutine = StartCoroutine(Transition(target.anchoredPosition, upPosition));
         if(!isVisible) onOpened?.Invoke();
         isVisible = true;
      }

      public void Close()
      {
         if (_isClosing) return;
         _isOpening = false;
         _isClosing = true;
         if (_actionRoutine != null)
         {
            _currentTransitionTime = transitionDuration - _currentTransitionTime;
            StopCoroutine(_actionRoutine);
         }
         isVisible = false;

         _actionRoutine = StartCoroutine(Transition(target.anchoredPosition, downPosition, true));
      }
   
      private IEnumerator Transition(Vector3 start, Vector3 end, bool wasClose = false)
      {
         while (_currentTransitionTime < transitionDuration)
         {
            target.anchoredPosition = Vector2.LerpUnclamped(start, end, transitionCurve.Evaluate(_currentTransitionTime / transitionDuration));
            _currentTransitionTime += Time.deltaTime;
            yield return null;
         }

         target.anchoredPosition = end;
         _currentTransitionTime = 0;
         _actionRoutine = null;
         if(wasClose) onClosed?.Invoke();
         _isClosing = false;
         _isOpening = false;
      }
   }
}
