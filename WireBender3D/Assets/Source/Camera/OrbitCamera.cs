using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    public float xSpeed = 8.0f;
    public float ySpeed = 8.0f;
    public float movementSpeed = 5.0f; // for pan()
    public float zoomSpeed = 5.0f; // for zooming with wheel of mouse
    
    [SerializeField] 
    private InputActionProperty movementClick = new InputActionProperty(new InputAction("forward", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty forward = new InputActionProperty(new InputAction("forward", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty backwards = new InputActionProperty(new InputAction("backwards", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty left = new InputActionProperty(new InputAction("left", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty right = new InputActionProperty(new InputAction("right", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty up = new InputActionProperty(new InputAction("up", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty down = new InputActionProperty(new InputAction("down", type: InputActionType.Button));
    [SerializeField] 
    private InputActionProperty mouseHorizontal = new InputActionProperty(new InputAction("mouseHorizontal", type: InputActionType.Value));
    [SerializeField] 
    private InputActionProperty mouseVertical = new InputActionProperty(new InputAction("mouseVertical", type: InputActionType.Value));

    private float _mouseX = 0.0f;
    private float _mouseY = 0.0f;
    private float moveSpeed = 1.5f;

    private Vector2 _horizontal = Vector2.zero;
    private Vector2 _vertical = Vector2.zero;
    private Vector2 _forward = Vector2.zero;
    private bool _movementEnabled = false;

    private Vector3 _eulerAngles = Vector3.zero;
    
    private void Start()
    {
        _eulerAngles = transform.eulerAngles;
        
        movementClick.action.performed += OnMovement;
        movementClick.action.canceled += OnMovement;
        forward.action.performed += OnForward;
        forward.action.canceled += OnForward;
        backwards.action.performed += OnBackwards;
        backwards.action.canceled += OnBackwards;
        left.action.performed += OnLeft;
        left.action.canceled += OnLeft;
        right.action.performed += OnRight;
        right.action.canceled += OnRight;
        up.action.performed += OnUp;
        up.action.canceled += OnUp;
        down.action.performed += OnDown;
        down.action.canceled += OnDown;
        mouseHorizontal.action.performed += OnRotationX;
        mouseHorizontal.action.canceled += OnRotationX;
        mouseVertical.action.performed += OnRotationY;
        mouseVertical.action.canceled += OnRotationY;
    }

    private void OnEnable()
    {
        movementClick.action.Enable();
        forward.action.Enable();
        backwards.action.Enable();
        left.action.Enable();
        right.action.Enable();
        up.action.Enable();
        down.action.Enable();
        mouseHorizontal.action.Enable();
        mouseVertical.action.Enable();
    }

    private void OnDisable()
    {
        movementClick.action.Disable();
        forward.action.Disable();
        backwards.action.Disable();
        left.action.Disable();
        right.action.Disable();
        up.action.Disable();
        down.action.Disable();
        mouseHorizontal.action.Disable();
        mouseVertical.action.Disable();
    }

    private void OnDestroy()
    {
        movementClick.action.performed -= OnMovement;
        movementClick.action.canceled -= OnMovement;
        forward.action.performed -= OnForward;
        forward.action.canceled -= OnForward;
        backwards.action.performed -= OnBackwards;
        backwards.action.canceled -= OnBackwards;
        left.action.performed -= OnLeft;
        left.action.canceled -= OnLeft;
        right.action.performed -= OnRight;
        right.action.canceled -= OnRight;
        up.action.performed -= OnUp;
        up.action.canceled -= OnUp;
        down.action.performed -= OnDown;
        down.action.canceled -= OnDown;
        mouseHorizontal.action.performed -= OnRotationX;
        mouseHorizontal.action.canceled -= OnRotationX;
        mouseVertical.action.performed -= OnRotationY;
        mouseVertical.action.canceled -= OnRotationY;
    }
    
    private void OnMovement(InputAction.CallbackContext obj)
    {
        bool isReleased = Mathf.Approximately(obj.ReadValue<float>(), 0.0f);
        _movementEnabled = !isReleased;
    }

    private void OnForward(InputAction.CallbackContext obj)
    {
        float press = obj.ReadValue<float>();
        _forward.y = press;
    }

    private void OnBackwards(InputAction.CallbackContext obj)
    {
        float press = obj.ReadValue<float>();
        _forward.x = press;
    }

    private void OnLeft(InputAction.CallbackContext obj)
    {
        float press = obj.ReadValue<float>();
        _horizontal.x = press;
    }

    private void OnRight(InputAction.CallbackContext obj)
    {
        float press = obj.ReadValue<float>();
        _horizontal.y = press;
    }

    private void OnUp(InputAction.CallbackContext obj)
    {
         float press = obj.ReadValue<float>();
         _vertical.y = press;
    }

    private void OnDown(InputAction.CallbackContext obj)
    {
        float press = obj.ReadValue<float>();
        _vertical.x = press;
    }

    private void OnRotationX(InputAction.CallbackContext obj)
    {
        _mouseX = obj.ReadValue<float>();
    }
    
    private void OnRotationY(InputAction.CallbackContext obj)
    {
        _mouseY = obj.ReadValue<float>();
    }

    void LateUpdate()
    {
        if (_movementEnabled)
        {
            orbit();
        }
        panMouseWheel();
        HandleZoomMouse();
    }

    private void panMouseWheel()
    {
        // Simplified operation to Pan by pressing the wheel button on the mouse.
        if (Input.GetMouseButton(2)) {
            transform.Translate(Vector3.right * -Input.GetAxis("Mouse X") * moveSpeed);
            transform.Translate(transform.up * -Input.GetAxis("Mouse Y") * moveSpeed, Space.World);
        }
    }
    
    private void HandleZoomMouse() {
        // Simplified operation to zoom in or out with the mouse wheel.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(transform.forward * scroll * zoomSpeed, Space.World);
    }
    
    private void orbit()
    {
        // Set rotation
        _eulerAngles.y = (_eulerAngles.y + xSpeed * _mouseX * Time.deltaTime + 360.0f) % 360.0f;
        _eulerAngles.x = (_eulerAngles.x -1.0f * ySpeed * _mouseY * Time.deltaTime + 360.0f) % 360.0f;

        // Camera based movement
        Vector3 cameraForward = transform.forward;
        Vector3 cameraRight = transform.right;
        Vector3 cameraUp = transform.up;

        Vector3 forwardRelativeMovement = Time.deltaTime * movementSpeed * (_forward.y - _forward.x) * cameraForward;
        Vector3 rightRelativeMovement = Time.deltaTime * movementSpeed * (_horizontal.y - _horizontal.x) * cameraRight;
        Vector3 upRelativeMovement = Time.deltaTime * movementSpeed * (_vertical.y - _vertical.x) * cameraUp;
        
        // Set position
        Vector3 relativeMovement = forwardRelativeMovement + rightRelativeMovement + upRelativeMovement;
        
        // Apply
        transform.position += relativeMovement;
        transform.eulerAngles = _eulerAngles;
    }
}
