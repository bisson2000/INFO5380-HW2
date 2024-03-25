using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireCreatorInput : MonoBehaviour
{
    // Create Curve
    [SerializeField] 
    private InputActionProperty createCurveAction = new InputActionProperty(new InputAction("Create curve", type: InputActionType.Button));
    // Extend Curvature
    [SerializeField] 
    private InputActionProperty extendCurvature = new InputActionProperty(new InputAction("Extend curvature of selected curve", type: InputActionType.Button));
    // Retract Curvature
    [SerializeField] 
    private InputActionProperty retractCurvature = new InputActionProperty(new InputAction("Retract curvature of selected curve", type: InputActionType.Button));
    // Create Line
    [SerializeField] 
    private InputActionProperty createLineAction = new InputActionProperty(new InputAction("Create straight segment", type: InputActionType.Button));
    // Extend Line
    [SerializeField] 
    private InputActionProperty extendLine = new InputActionProperty(new InputAction("Extend selected straight line segment", type: InputActionType.Button));
    // Retract Line
    [SerializeField] 
    private InputActionProperty retractLine = new InputActionProperty(new InputAction("Reduce selected straight line segment", type: InputActionType.Button));
    // Erase Segment
    [SerializeField] 
    private InputActionProperty eraseSegment = new InputActionProperty(new InputAction("Erase selected segment", type: InputActionType.Button));
    // Rotate Segment Clockwise
    [SerializeField] 
    private InputActionProperty rotateSegmentClockwise = new InputActionProperty(new InputAction("Rotate Selected Segment Clockwise", type: InputActionType.Button));
    // Rotate Segment Counter-Clockwise
    [SerializeField] 
    private InputActionProperty rotateSegmentCounterClockwise = new InputActionProperty(new InputAction("Rotate Selected Segment Counter-Clockwise", type: InputActionType.Button));
    // Select Next Segment
    [SerializeField] 
    private InputActionProperty selectNextSegment = new InputActionProperty(new InputAction("Select Next Segment", type: InputActionType.Button));
    // Select Previous Segment
    [SerializeField] 
    private InputActionProperty selectPreviousSegment = new InputActionProperty(new InputAction("Select Prvious Segment", type: InputActionType.Button));
    // Print coordinates of Points to in a list to terminal in Unity
    [SerializeField] 
    private InputActionProperty printCoordinatesAsArray = new InputActionProperty(new InputAction("Select Prvious Segment", type: InputActionType.Button));
    // Save coordinates to /Output/Coordinates.csv
    [Tooltip("Export Coordinates to CSV")]
    [SerializeField] 
    private InputActionProperty exportCoordinates2CSV = new InputActionProperty(new InputAction("Select Previous Segment", type: InputActionType.Button));

    // private WireCreator _wireCreator;

    private WireUserCreator _wireUserCreator;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireUserCreator = GetComponent<WireUserCreator>();
        createCurveAction.action.performed += OnCreateCurve; 
        extendCurvature.action.performed += OnExtendCurvature;
        retractCurvature.action.performed += OnRetractCurvature;
        createLineAction.action.performed += OnCreateLine; 
        extendLine.action.performed += OnExtendLine;
        retractLine.action.performed += OnRetractLine;
        eraseSegment.action.performed += OnEraseSegment; 
        rotateSegmentClockwise.action.performed += OnRotateSegmentClockwise; 
        rotateSegmentCounterClockwise.action.performed += OnRotateSegmentCounterClockwise;
        selectNextSegment.action.performed += OnSelectNextSegment;
        selectPreviousSegment.action.performed += OnSelectPreviousSegment;
        printCoordinatesAsArray.action.performed += OnPrintCoordinatesAsArray;
        exportCoordinates2CSV.action.performed += OnExportCoordinates2CSV;
    }

    private void OnCreateLine(InputAction.CallbackContext obj)
    {
        if (!IsShiftHeld() && !IsDeletePressed()) // If Shift is not held
        {
            _wireUserCreator.AddNewLine(); // Perform action only if Left or Right Shift key and Delete key is NOT pressed down. Mapped Key in Input Action (Key Mapped: "L")
        }
    }
    private void OnExtendLine(InputAction.CallbackContext obj)
    {
        if (IsShiftHeld())
        {
            // Perform action only if Left or Right Shift Key is pressed down + Mapped Key in Input Action (Key Mapped: "L")
            _wireUserCreator.ExtendLine(0.1f);
        }
    }
    private void OnRetractLine(InputAction.CallbackContext obj)
    {
        if (IsDeletePressed())
        {
            // Perform action only if "Delete" Key is pressed down + Mapped Key in Input Action (Key Mapped: "L")
            _wireUserCreator.ExtendLine(-0.1f);
        }
    }
    private void OnCreateCurve(InputAction.CallbackContext obj)
    {
        if (!IsShiftHeld() && !IsDeletePressed()) // Neither Shift nor Delete can be held to create a new curve
        {
            _wireUserCreator.AddNewCurve(); // Perform action only if Left or Right Shift Key is NOT pressed down. Mapped Key in Input Action (Key Mapped: "C")
        }
    }
    private void OnExtendCurvature(InputAction.CallbackContext obj)
    {
        if (IsShiftHeld())
        {
            // Perform action only if Left or Right Shift Key is pressed down + Mapped Key in Input Action (Key Mapped: "C")
            _wireUserCreator.ExtendCurvature(15.0f);
        }
    }
    private void OnRetractCurvature(InputAction.CallbackContext obj)
    {
        if (IsDeletePressed())
        {
            // Perform action only if "Delete" key is pressed down + Mapped Key in Input Action (Key Mapped: "C")
            _wireUserCreator.ExtendCurvature(-15.0f);
        }
    }
    private void OnEraseSegment(InputAction.CallbackContext obj) // Mapped Key in Input Action (Key Mapped: "Backspace")
    {
        _wireUserCreator.EraseSegment();
    }
    private void OnRotateSegmentClockwise(InputAction.CallbackContext obj)
    {
        if (!IsShiftHeld()) // Perform action only if Left or Right Shift Key is NOT pressed down + Mapped Key in Input Action (Key Mapped: "R")
        {
            _wireUserCreator.RotateSegmentClockwise(15.0f);
        }
    }
    private void OnRotateSegmentCounterClockwise(InputAction.CallbackContext obj)
    {
        if (IsShiftHeld()) // Perform action only if Left or Right Shift Key is pressed down + Mapped Key in Input Action (Key Mapped: "R")
        {
            _wireUserCreator.RotateSegmentClockwise(-15.0f);
        }
    }
    private void OnSelectNextSegment(InputAction.CallbackContext obj) // Mapped Key in Input Action (Key Mapped: "Up Arrow key")
    {
        _wireUserCreator.SelectNextSegment();
    }
    private void OnSelectPreviousSegment(InputAction.CallbackContext obj) // Mapped Key in Input Action (Key Mapped: "Down Arrow key")
    {
        _wireUserCreator.SelectPreviousSegment();
    }
    private void OnPrintCoordinatesAsArray(InputAction.CallbackContext obj)
    {
        if (!IsShiftHeld()) // Perform action only if Left or Right Shift Key is NOT pressed down + Mapped Key in Input Action (Key Mapped: "P")
        {
            _wireUserCreator.PrintCoordinatesAsArray();
        }
    }
    private void OnExportCoordinates2CSV(InputAction.CallbackContext obj)
    {
        if (IsShiftHeld()) // Perform action only if Left or Right Shift Key is pressed down + Mapped Key in Input Action (Key Mapped: "R")
        {
            _wireUserCreator.ExportCoordinates2CSV();
        }
    }
    
    private void OnEnable()
    {
        createCurveAction.action.Enable();
        createLineAction.action.Enable();
        extendLine.action.Enable();
        retractLine.action.Enable();
        eraseSegment.action.Enable();
        rotateSegmentClockwise.action.Enable(); 
        rotateSegmentCounterClockwise.action.Enable();
        extendCurvature.action.Enable();
        retractCurvature.action.Enable();
        selectNextSegment.action.Enable();
        selectPreviousSegment.action.Enable();
        printCoordinatesAsArray.action.Enable();
        exportCoordinates2CSV.action.Enable();
    }

    private void OnDisable()
    {
        createCurveAction.action.Disable();
        createLineAction.action.Disable();
        extendLine.action.Disable();
        retractLine.action.Disable();
        eraseSegment.action.Disable();
        rotateSegmentClockwise.action.Disable();
        rotateSegmentCounterClockwise.action.Disable();
        extendCurvature.action.Disable();
        retractCurvature.action.Disable();
        selectNextSegment.action.Disable();
        selectPreviousSegment.action.Disable();
        printCoordinatesAsArray.action.Disable();
        exportCoordinates2CSV.action.Disable();
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
        extendLine.action.performed -= OnExtendLine;
        retractLine.action.performed -= OnRetractLine;
        selectNextSegment.action.performed -= OnSelectNextSegment;
        selectPreviousSegment.action.performed -= OnSelectPreviousSegment;
        printCoordinatesAsArray.action.performed -= OnPrintCoordinatesAsArray;
        exportCoordinates2CSV.action.performed -= OnExportCoordinates2CSV;
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
