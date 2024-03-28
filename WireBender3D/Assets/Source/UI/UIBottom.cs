using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIBottom : MonoBehaviour
{
    [SerializeField] private WireUserCreator _wireUserCreator;
    [SerializeField] private WireInfo _wireInfo;
    [SerializeField] private MachineCollisions _machineCollisions;
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
        Debug.Log("Coordinates saved to WireBender3D/Assets/Output/Coordinates.csv");
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
