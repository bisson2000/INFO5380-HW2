using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireCreatorInput : MonoBehaviour
{
    // Create Curve
    [SerializeField] 
    private InputActionProperty createCurveAction = new InputActionProperty(new InputAction("Create curve", type: InputActionType.Button));
    // Create Line
    [SerializeField] 
    private InputActionProperty createLineAction = new InputActionProperty(new InputAction("Create straight segment", type: InputActionType.Button));
    // Erase Segment
    [SerializeField] 
    private InputActionProperty eraseSegment = new InputActionProperty(new InputAction("Erase selected segment", type: InputActionType.Button));
    // Rotate Segment Clockwise
    [SerializeField] 
    private InputActionProperty rotateSegmentClockwise = new InputActionProperty(new InputAction("Erase selected segment", type: InputActionType.Button));
    // Rotate Segment Counter-Clockwise
    [SerializeField] 
    private InputActionProperty rotateSegmentCounterClockwise = new InputActionProperty(new InputAction("Erase selected segment", type: InputActionType.Button));
    // Extend Curvature
    [SerializeField] 
    private InputActionProperty extendCurvature = new InputActionProperty(new InputAction("Erase selected segment", type: InputActionType.Button));
    // Retract Curvature
    [SerializeField] 
    private InputActionProperty retractCurvature = new InputActionProperty(new InputAction("Erase selected segment", type: InputActionType.Button));

    
    // private WireCreator _wireCreator;

    private WireUserCreator _wireUserCreator;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireUserCreator = GetComponent<WireUserCreator>();
        createCurveAction.action.performed += OnCreateCurve; 
        createLineAction.action.performed += OnCreateLine; 
        eraseSegment.action.performed += OnEraseSegment; 
        rotateSegmentClockwise.action.performed += OnRotateSegmentClockwise; 
        rotateSegmentCounterClockwise.action.performed += OnRotateSegmentCounterClockwise;
        extendCurvature.action.performed += OnExtendCurvature;
        retractCurvature.action.performed += OnRetractCurvature;
    }

    private void OnCreateLine(InputAction.CallbackContext obj)
    {
        if (!IsShiftHeld()) // If Shift is not held
        {
            _wireUserCreator.AddNewLine(); // Perform action only if Left or Right Shift Key is NOT pressed down. Mapped Key in Input Action (Key Mapped: "L")
        }
    }
    private void OnCreateCurve(InputAction.CallbackContext obj)
    {
        if (!IsShiftHeld()) // If Shift is not held
        {
            _wireUserCreator.AddNewCurve(); // Perform action only if Left or Right Shift Key is NOT pressed down. Mapped Key in Input Action (Key Mapped: "C")
        }
    }
    private void OnEraseSegment(InputAction.CallbackContext obj)
    {
        _wireUserCreator.EraseSegment();
    }
    private void OnRotateSegmentClockwise(InputAction.CallbackContext obj)
    {
        _wireUserCreator.RotateSegmentClockwise();
    }
    private void OnRotateSegmentCounterClockwise(InputAction.CallbackContext obj)
    {
        _wireUserCreator.RotateSegmentCounterClockwise();
    }
    private void OnExtendCurvature(InputAction.CallbackContext obj)
    {
        if (IsShiftHeld())
        {
            // Perform action only if Left or Right Shift Key is pressed down + Mapped Key in Input Action (Key Mapped: "C")
            _wireUserCreator.ExtendCurvature();
        }
    }
    private void OnRetractCurvature(InputAction.CallbackContext obj)
    {
        if (IsDeletePressed())
        {
            // Perform action only if Left or Right Shift Key is pressed down + Mapped Key in Input Action (Key Mapped: "C")
            _wireUserCreator.ExtendCurvature();
        }
    }
    private void OnEnable()
    {
        createCurveAction.action.Enable();
        createLineAction.action.Enable();
        eraseSegment.action.Enable();
        rotateSegmentClockwise.action.Enable(); 
        rotateSegmentCounterClockwise.action.Enable();
        extendCurvature.action.Enable();
        retractCurvature.action.Enable();
    }

    private void OnDisable()
    {
        createCurveAction.action.Disable();
        createLineAction.action.Disable();
        eraseSegment.action.Disable();
        rotateSegmentClockwise.action.Disable();
        rotateSegmentCounterClockwise.action.Disable();
        extendCurvature.action.Disable();
        retractCurvature.action.Disable();
    }

    private void OnDestroy()
    {
        createCurveAction.action.performed -= OnCreateCurve; 
        createLineAction.action.performed -= OnCreateLine;  
        eraseSegment.action.performed -= OnEraseSegment; 
        rotateSegmentClockwise.action.performed -= OnRotateSegmentClockwise; 
        rotateSegmentCounterClockwise.action.performed -= OnRotateSegmentCounterClockwise; 
        extendCurvature.action.performed -= OnExtendCurvature;
        retractCurvature.action.performed -= OnRetractCurvature;
    }
    
    // Helper Functions to Map two Keyboard keys to Input Action
    private bool IsShiftHeld() // Returns True if Either Shift Key is Pressed
    {
        return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }
    private bool IsDeletePressed() // Returns True if Delete Key is Pressed
    {
        return Keyboard.current.deleteKey.isPressed;
    }



    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
