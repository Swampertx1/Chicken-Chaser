using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.General
{
    public class BestVirtualCursor : UIBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Graphic graphic;
        [SerializeField] private float speed = 400;

        private Mouse _mVirtualMouse;
        private PlayerInput _owner;
        private CanvasScaler _scaler;
        private RectTransform _rectTransform;
        private Camera _camera;
        
        
        private Vector2 _min;
        private Vector2 _max;
        private Vector2 _movementVector;
        private Vector2 _offset;
        public Mouse Mouse => _mVirtualMouse;

        protected override void OnEnable()
        {
            base.OnEnable();
    
            if (_owner)
            {
                if (_owner.currentControlScheme != "Controller")
                {
                    Debug.Log("Destroying cursor as we're not using a controller");
                    Destroy(gameObject);
                    return;
                }
                
                _owner.onDeviceLost += OnPlayerChanged;
                _owner.onDeviceRegained += OnPlayerChanged;
                
            }
            
            
            //TryAddDevice();

            //SplitscreenPlayerManager.Instance.OnClientsUpdated += OnRectTransformDimensionsChange;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_mVirtualMouse != null && _mVirtualMouse.added)
                InputSystem.RemoveDevice(_mVirtualMouse);
    
            if (_owner)
            {
                _owner.onDeviceLost -= OnPlayerChanged;
                _owner.onDeviceRegained -= OnPlayerChanged;
            }
            //SplitscreenPlayerManager.Instance.OnClientsUpdated += OnRectTransformDimensionsChange;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            
            base.Start();
            TryAddDevice();
            
            PlayerInput root  = transform.root.GetComponent<PlayerInput>();
            _rectTransform = graphic.rectTransform;
            
            Debug.Log("VIrtual Cursor attaching to: " + root.transform.name, root.gameObject);
            //SplitscreenPlayerManager.Instance.OnPlayerConnected += OnPlayerChanged;
            //SplitscreenPlayerManager.Instance.OnPlayerDisconnected += OnPlayerChanged;
            OnPlayerChanged(root);

            root.actions["VirtualMove"].performed += HandleMove;
            root.actions["VirtualInteract"].performed += HandleInteract;
            root.actions["VirtualScroll"].performed += HandleScroll;
            OnRectTransformDimensionsChange();
        }

        private void TryAddDevice()
        {
            if (_mVirtualMouse == null)
            {
                _mVirtualMouse = (Mouse)InputSystem.AddDevice( "VirtualMouse", transform.root.name+"_VirtualMouse");
                Debug.Log("Creating Virtual Mouse");
                
            }
            /*
            else if (!_mVirtualMouse.added)
            {
                InputSystem.AddDevice(_mVirtualMouse);
                Debug.Log("Adding virtual mouse");
            }
            */
        }

        private void HandleInteract(InputAction.CallbackContext obj)
        {
            Debug.Log("Trying to HandleInteracts");

            var mouseState = new MouseState
            {
                buttons = (ushort) (obj.ReadValueAsButton()?1:0)// 1 for pressed, 0 for released
            };
            InputSystem.QueueStateEvent(_mVirtualMouse, mouseState);
        }
        
        private void HandleScroll(InputAction.CallbackContext obj)
        {
            Vector2 scrollDelta = obj.ReadValue<Vector2>();

            if (scrollDelta != Vector2.zero)
            {
                InputState.Change(_mVirtualMouse.scroll, scrollDelta);
            }
        } 
        private void HandleMove(InputAction.CallbackContext obj)
        {
            
            _movementVector = obj.ReadValue<Vector2>();
        }

        private void OnPlayerChanged(PlayerInput obj)
        {
            // If an owner is already set, do nothing.
            if (_owner)
            {
                SetVisibility();
                OnRectTransformDimensionsChange();
                return;
            }
            _owner = obj;

            graphic.color = _owner.playerIndex switch
            {
                0 => Color.red,
                1 => Color.green,
                2 => Color.blue,
                3 => Color.yellow,
                4 => Color.magenta,
                _ => graphic.color
            };
            SetVisibility();
            
            //Required for UI interaction.
            InputUser.PerformPairingWithDevice(_mVirtualMouse.device, _owner.user);
            
            
            OnRectTransformDimensionsChange();
            
            _owner.onDeviceLost += OnPlayerChanged;
            _owner.onDeviceRegained += OnPlayerChanged;
            
        }
        
        private void SetVisibility()
        {
            if (_owner.currentControlScheme == "Controller")
            {
                graphic.enabled = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                graphic.enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_mVirtualMouse == null || !canvas)return;
            _rectTransform ??= graphic.rectTransform;
            _camera ??= canvas.worldCamera;
            
            
            RectTransform canvasRectTransform = (RectTransform)canvas.transform;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.localScale = Vector3.one * (1f / canvasRectTransform.localScale.x);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _scaler ??= canvas.GetComponent<CanvasScaler>();

                // Get viewport size in pixels (accounts for split-screen)
                float viewportHeight = _camera.pixelHeight;
                float viewportWidth = _camera.pixelWidth;

                // Compute scale factor based on the player's viewport size
                float scaleX = _scaler.referenceResolution.x / viewportWidth;
                float scaleY = _scaler.referenceResolution.y / viewportHeight;

                // Blend scaling based on CanvasScaler matchWidthOrHeight
                float match = _scaler.matchWidthOrHeight;
                float scaleFactor = Mathf.Lerp(scaleX, scaleY, match);

                // Apply scale
                transform.localScale = Vector3.one * scaleFactor;
                canvasRectTransform.sizeDelta *= (1f / scaleFactor);

            }


            Rect r = _rectTransform.rect;
            _min = new Vector2(-r.xMin, -r.yMin);
            
            _offset = new Vector2(Screen.width* _camera.rect.x, Screen.height * _camera.rect.y); 
            _max = new Vector2(_camera.pixelWidth- r.xMax,_camera.pixelHeight-r.yMax);

            
            Vector2 center = (_max-_min)*0.5f;
            _rectTransform.anchoredPosition = center;
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 virtualMousePosition = _rectTransform.anchoredPosition + _movementVector * (speed * Time.deltaTime);
            Vector2 newMousePos = virtualMousePosition + _offset;
            if (_mVirtualMouse.position.ReadValue() != newMousePos)
            {
                virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, _min.x, _max.x); 
                virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, _min.y, _max.y);
                _rectTransform.anchoredPosition = virtualMousePosition;
                Vector2 set = _rectTransform.anchoredPosition + _offset;
                InputState.Change(_mVirtualMouse.position, set);
                _mVirtualMouse.WarpCursorPosition(set);
            }
        }

    }
}