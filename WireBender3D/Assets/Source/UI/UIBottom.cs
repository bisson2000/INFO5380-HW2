using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.InputSystem;

public class UIBottom : MonoBehaviour
{
    [SerializeField] private WireUserCreator _wireUserCreator;
    [SerializeField] private WireInfo _wireInfo;
    [SerializeField] private MachineCollisions _machineCollisions;
    
    // Add arrays for buttons
    public List<Button> buttonsForCurveOnly;
    public List<Button> buttonsForLineOnly;
    public List<Button> buttonsForBoth;
    
    public void OnToggleMachineCollisions() // Mapped Key in Input Action (Key Mapped: "Backspace")
    {
        _machineCollisions.ToggleMachineCollisions();
    }
    public void OnTighten()
    {
        _wireUserCreator.ExtendCurveDistanceFromCenter(-0.1f);
        Debug.Log("Tighten curvature");
    }
    public void OnLoosen()
    {
        _wireUserCreator.ExtendCurveDistanceFromCenter(0.1f);
        Debug.Log("Loosen curvature");
    }
    public void OnWireInfo()
    {
        _wireInfo.ToggleInfo();
        Debug.Log("Displayed Wire Info");
    }
    public void OnButtonAddNewLine()
    {
        _wireUserCreator.AddNewLine();
        Debug.Log("Added New Line Segment");
    }
    public void OnButtonExtendLine()
    {
        
        _wireUserCreator.ExtendLine(0.1f);
        Debug.Log("Extended Line");
    }
    public void OnButtonRetractLine()
    {
        _wireUserCreator.ExtendLine(-0.1f);
        Debug.Log("Retracted Line");
    }
    public void OnValueChanged(string stringValue)
    {
        bool parsed = float.TryParse(stringValue, out float result);
        if (!parsed)
        {
            return;
        }
        Debug.Log(result);
        _wireUserCreator.SetLineLength(result);
    }
    public void OnButtonAddNewCurve()
    {
        _wireUserCreator.AddNewCurve();
        Debug.Log("Added New Curve");
    }
    public void OnButtonExtendCurvature()
    {
        _wireUserCreator.ExtendCurvature(15.0f);
        Debug.Log("Extended Curvature");
    }
    public void OnButtonRetractCurvature()
    {
        _wireUserCreator.ExtendCurvature(-15.0f);
        Debug.Log("Retracted Curvature");
    }
    public void OnButtonEraseSegment()
    {
        _wireUserCreator.EraseSegment();
        Debug.Log("Erased Selected Segment");
    }
    public void OnButtonRotateSegmentClockwise()
    {
        _wireUserCreator.RotateSegmentClockwise(15.0f);
        Debug.Log("Rotated Selected Segment Clockwise");
    }
    public void OnButtonRotateSegmentCounterClockwise()
    {
        _wireUserCreator.RotateSegmentClockwise(-15.0f);
        Debug.Log("Rotated Selected Segment Counter-Clockwise");
    }
    public void OnButtonExportCoordinates2CSV()
    {
        _wireUserCreator.ExportCoordinates2CSV();
        Debug.Log("Coordinates saved to WireBender3D/Assets/Coordinates.csv");
    }
    
    // public void OnButtonPressed()
    // {
    //     Debug.Log("Click");
    //     _wireUserCreator.AddNewLine();
    // }
    // public void OnButtonPressed()
    // {
    //     Debug.Log("Click");
    //     _wireUserCreator.AddNewCurve();
    // }
    // Start is called before the first frame update
    void Start()
    {
        _wireUserCreator.OnSelectionChange += OnSelectionChange;
        SetButtonsActive(buttonsForCurveOnly, false); // Initially hide curve buttons
        SetButtonsActive(buttonsForLineOnly, false);  // Initially hide line buttons
        SetButtonsActive(buttonsForBoth, false);
    }

    private void OnSelectionChange()
    {
        int segment = _wireUserCreator.SelectedSegment;
        if (segment >= 0 && segment < _wireUserCreator.SegmentList.Count)
        {
            Segment iSegment = _wireUserCreator.SegmentList[segment];
            if (iSegment is Curve)
            {
                SetButtonsActive(buttonsForCurveOnly, true);  // Show buttons for curves
                SetButtonsActive(buttonsForLineOnly, false); // Hide buttons for lines
            }
            else if (iSegment is Line)
            {
                SetButtonsActive(buttonsForLineOnly, true);  // Show buttons for lines
                SetButtonsActive(buttonsForCurveOnly, false); // Hide buttons for curves
            }
            SetButtonsActive(buttonsForBoth, true);
        }
        else
        {
            // If no valid segment, hide all buttons
            SetButtonsActive(buttonsForCurveOnly, false);
            SetButtonsActive(buttonsForLineOnly, false);
            SetButtonsActive(buttonsForBoth, false);
        }
    }
    // Utility method to set the active state of a list of buttons
    private void SetButtonsActive(List<Button> buttons, bool isInteractible)
    {
        foreach (Button button in buttons)
        {
            button.interactable = isInteractible;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
