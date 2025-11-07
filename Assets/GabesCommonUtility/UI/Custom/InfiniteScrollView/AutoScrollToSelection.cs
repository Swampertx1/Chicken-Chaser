using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GabesCommonUtility.UI.Custom.InfiniteScrollView
{
    public class AutoScrollToSelection : MonoBehaviour
    {
        [Header("Scroll View References")] [SerializeField]
        private ScrollRect scrollRect;

        [Header("Settings")] [SerializeField] private float scrollSpeed = 5f;
        [SerializeField] private bool smoothScroll = true;
        [SerializeField] private float padding = 10f;

        private RectTransform _contentTransform;
        private RectTransform _viewportTransform;
        private GameObject _currentSelectedObject;
        private Vector2 _targetScrollPosition;
        private bool _isScrolling;

        void Start()
        {
            scrollRect ??= GetComponent<ScrollRect>();

            if (scrollRect != null)
            {
                _contentTransform = scrollRect.content;
                _viewportTransform = scrollRect.viewport;
            }


        }

        void Update()
        {
            GameObject selectedObject = EventSystem.current?.currentSelectedGameObject;

            if (selectedObject != _currentSelectedObject)
            {
                _currentSelectedObject = selectedObject;

                if (_currentSelectedObject != null && IsObjectInScrollView(_currentSelectedObject))
                {
                    ScrollToObject(_currentSelectedObject);
                }
            }

            if (_isScrolling && smoothScroll)
            {
                Vector2 newPos = Vector2.Lerp(scrollRect.normalizedPosition, _targetScrollPosition,
                    scrollSpeed * Time.deltaTime);
                scrollRect.normalizedPosition = newPos;

                if (Vector2.Distance(newPos, _targetScrollPosition) < 0.01f)
                {
                    scrollRect.normalizedPosition = _targetScrollPosition;
                    _isScrolling = false;
                }
            }
        }

        private bool IsObjectInScrollView(GameObject obj)
        {
            return true;
            /*
            if (_contentTransform == null || obj == null) return false;

            Transform current = obj.transform;
            while (current != null)
            {
                if (current == _contentTransform) return true;
                current = current.parent;
            }
            return false;
            */
        }

        public void ScrollToObject(GameObject targetObject)
        {
            if (scrollRect == null || _contentTransform == null || _viewportTransform == null || targetObject == null)
                return;

            RectTransform targetRect = targetObject.GetComponent<RectTransform>();
            if (targetRect == null) return;

            Vector2 newScrollPosition = CalculateScrollPosition(targetRect);

            if (smoothScroll)
            {
                _targetScrollPosition = newScrollPosition;
                _isScrolling = true;
            }
            else
            {
                scrollRect.normalizedPosition = newScrollPosition;
            }
        }

        private Vector2 CalculateScrollPosition(RectTransform targetRect)
        {

            // Get the current scroll position
            Vector2 currentScrollPos = scrollRect.normalizedPosition;
            Vector2 newScrollPos = currentScrollPos;

            // Get viewport world corners and convert to content local space
            Vector3[] viewportCorners = new Vector3[4];
            Vector3[] targetCorners = new Vector3[4];

            _viewportTransform.GetWorldCorners(viewportCorners);
            targetRect.GetWorldCorners(targetCorners);

            // Convert to content local space
            for (int i = 0; i < 4; i++)
            {
                viewportCorners[i] = _contentTransform.InverseTransformPoint(viewportCorners[i]);
                targetCorners[i] = _contentTransform.InverseTransformPoint(targetCorners[i]);
            }

            // Get bounds
            float viewportLeft = viewportCorners[0].x;
            float viewportRight = viewportCorners[2].x;
            float viewportBottom = viewportCorners[0].y;
            float viewportTop = viewportCorners[2].y;

            float targetLeft = targetCorners[0].x;
            float targetRight = targetCorners[2].x;
            float targetBottom = targetCorners[0].y;
            float targetTop = targetCorners[2].y;

            // Get content bounds
            Rect contentRect = _contentTransform.rect;
            float contentWidth = contentRect.width;
            float contentHeight = contentRect.height;

            // Calculate horizontal scroll
            if (scrollRect.horizontal && contentWidth > (viewportRight - viewportLeft))
            {
                float scrollableWidth = contentWidth - (viewportRight - viewportLeft);

                if (targetLeft < viewportLeft + padding)
                {
                    // Need to scroll left
                    float desiredLeft = targetLeft - padding;
                    float scrollOffset = viewportLeft - desiredLeft;
                    float currentScrollValue = currentScrollPos.x * scrollableWidth;
                    newScrollPos.x = Mathf.Clamp01((currentScrollValue - scrollOffset) / scrollableWidth);
                }
                else if (targetRight > viewportRight - padding)
                {
                    // Need to scroll right
                    float desiredRight = targetRight + padding;
                    float scrollOffset = desiredRight - viewportRight;
                    float currentScrollValue = currentScrollPos.x * scrollableWidth;
                    newScrollPos.x = Mathf.Clamp01((currentScrollValue + scrollOffset) / scrollableWidth);
                }
            }

            // Calculate vertical scroll
            if (scrollRect.vertical && contentHeight > (viewportTop - viewportBottom))
            {
                float scrollableHeight = contentHeight - (viewportTop - viewportBottom);

                if (targetTop > viewportTop - padding)
                {
                    // Need to scroll up
                    float desiredTop = targetTop + padding;
                    float scrollOffset = desiredTop - viewportTop;
                    float currentScrollValue = currentScrollPos.y * scrollableHeight;
                    newScrollPos.y = Mathf.Clamp01((currentScrollValue + scrollOffset) / scrollableHeight);
                }
                else if (targetBottom < viewportBottom + padding)
                {
                    // Need to scroll down
                    float desiredBottom = targetBottom - padding;
                    float scrollOffset = viewportBottom - desiredBottom;
                    float currentScrollValue = currentScrollPos.y * scrollableHeight;
                    newScrollPos.y = Mathf.Clamp01((currentScrollValue - scrollOffset) / scrollableHeight);
                }
            }

            return newScrollPos;
        }

        public void SelectAndScrollToObject(GameObject targetObject)
        {
            if (targetObject != null)
            {
                EventSystem.current.SetSelectedGameObject(targetObject);
                ScrollToObject(targetObject);
            }
        }
    }
}