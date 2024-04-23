using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class WireInfo : MonoBehaviour
{
    [SerializeField] 
    private WireUserCreator _wireUserCreator;
    private WireRenderer _wireUserRenderer;

    [SerializeField] 
    private GameObject informationPrefab;
    
    [SerializeField] 
    private Transform activationChild;

    [SerializeField] 
    private InputActionProperty displayInfoAction = new InputActionProperty(new InputAction("Display Info", type: InputActionType.Button));
    
    private List<GameObject> _createdInformation = new List<GameObject>();
    

    private float lineOffsetFromWire = 0.1f;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireUserRenderer = _wireUserCreator.gameObject.GetComponent<WireRenderer>();
        _wireUserRenderer.OnMeshGenerated += PlaceMeasurements;
        displayInfoAction.action.performed += ShowInfo;
    }

    private void OnDestroy()
    {
        _wireUserRenderer.OnMeshGenerated -= PlaceMeasurements;
        displayInfoAction.action.performed -= ShowInfo;

    }

    private void OnEnable()
    {
        displayInfoAction.action.Enable();
    }

    private void OnDisable()
    {
        displayInfoAction.action.Disable();
    }

    private void ShowInfo(InputAction.CallbackContext obj)
    {
        activationChild.gameObject.SetActive(!activationChild.gameObject.activeSelf);
    }
    public void ToggleInfo()
    {
        activationChild.gameObject.SetActive(!activationChild.gameObject.activeSelf);
    }


    private void PlaceMeasurements()
    {
        
        // Erase last information
        for (int i = 0; i < _createdInformation.Count; i++)
        {
            Destroy(_createdInformation[i]);
        }
        _createdInformation.Clear();
        
        if (_wireUserCreator.SegmentList.Count == 0)
        {
            return;
        }

        HashSet<int> coloredPositions = new HashSet<int>();
        int firstLineIndex = -1;
        for (int i = 0; i < _wireUserCreator.SegmentList.Count; i++)
        {
            Segment segment = _wireUserCreator.SegmentList[i];
            
            if (segment is Curve curve)
            {
                // Generate line info
                if (firstLineIndex != -1)
                {
                    int startLine = _wireUserCreator.SegmentList[firstLineIndex].StartPointIndex;
                    int endLine = _wireUserCreator.SegmentList[i].StartPointIndex;
                    GenerateLineInfo(startLine, endLine);
                    firstLineIndex = -1;
                }
                
                // Generate curve info
                GenerateCurveInfo(curve);
                
                int start = segment.StartPointIndex;
                int count = segment.EndPointIndex - start;
                coloredPositions.AddRange(Enumerable.Range(start, count));
                
            } 
            else if (segment is Line && firstLineIndex == -1)
            {
                firstLineIndex = i;
            }
        }

        if (firstLineIndex != -1)
        {
            int startLine = _wireUserCreator.SegmentList[firstLineIndex].StartPointIndex;
            int endLine = _wireUserCreator.SegmentList[^1].EndPointIndex;
            GenerateLineInfo(startLine, endLine);
        }
    }

    private float InformationOffset()
    {
        return _wireUserRenderer.Radius + lineOffsetFromWire;
    }

    private void GenerateLineInfo(int startIndex, int endindex)
    {
        (Vector3 start, Quaternion startRot) = _wireUserRenderer.GetPositionRotation(startIndex);
        Vector3 end = _wireUserRenderer.Positions[endindex];
        Vector3 up = WireRenderer.GetUp(start, startRot);

        GameObject go = Instantiate(informationPrefab, activationChild);
        go.GetComponent<WireInfoMediator>().SetLine(start, end, up, _wireUserRenderer.Radius);
        _createdInformation.Add(go);
    }

    private void GenerateCurveInfo(Curve curve)
    {
        // Get the pivot point
        float curvatureFlip = 1.0f;
        if (curve.CurvatureAngleDegrees < 0)
        {
            curvatureFlip = -1.0f;
            curve.CurvatureAngleDegrees *= -1.0f;
        }
        
        float angleStep = curve.CurvatureAngleDegrees / 2.0f;
        
        // Get the pivot point
        (Vector3 startPoint, Quaternion startRotation) = _wireUserRenderer.GetPositionRotation(curve.StartPointIndex);
        Vector3 startForward = WireRenderer.GetForward(startPoint, startRotation);
        // The original pivot direction
        Vector3 pivotDirection = WireRenderer.GetRight(startPoint, startRotation) * curvatureFlip;
        pivotDirection = Quaternion.AngleAxis(curve.AngleTwistDegrees, startForward) * pivotDirection * curve.DistanceFromCenter;
        Vector3 pivotPoint = pivotDirection + startPoint;
        
        // Get the rotation that must be completed around the pivot
        Vector3 rotationAxis = Vector3.Cross(startForward, pivotDirection.normalized);
        
        Quaternion rotationMiddle = Quaternion.AngleAxis(angleStep, rotationAxis);
        Quaternion rotationStart = Quaternion.AngleAxis(Math.Max(0, angleStep - 90.0f), rotationAxis);
        Quaternion rotationEnd = Quaternion.AngleAxis(Math.Min(curve.CurvatureAngleDegrees, angleStep + 90.0f), rotationAxis);
        
        // The curved points
        // Rotate the current point around the pivot
        Vector3 middle = rotationMiddle * (startPoint - pivotPoint) + pivotPoint;
        Vector3 start = rotationStart * (startPoint - pivotPoint) + pivotPoint;
        Vector3 end = rotationEnd * (startPoint - pivotPoint) + pivotPoint;

        Vector3 middleUp = (middle - pivotPoint).normalized;
        Vector3 startUp = (start - pivotPoint).normalized;
        Vector3 endUp = (end - pivotPoint).normalized;
        
        
        GameObject go = Instantiate(informationPrefab, activationChild);
        go.GetComponent<WireInfoMediator>().SetCurve(start, middle, end, middleUp, _wireUserRenderer.Radius, curve);
        _createdInformation.Add(go);
    }
}
