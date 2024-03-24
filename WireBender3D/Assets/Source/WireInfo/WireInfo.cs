using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class WireInfo : MonoBehaviour
{
    [SerializeField] 
    private WireUserCreator _wireUserCreator;
    private WireRenderer _wireUserRenderer;
    private WireRenderer _localWireRenderer;

    [SerializeField] 
    private GameObject informationPrefab;

    private List<GameObject> _createdInformation = new List<GameObject>();
    

    private float lineOffsetFromWire = 0.1f;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireUserRenderer = _wireUserCreator.gameObject.GetComponent<WireRenderer>();

        _localWireRenderer = GetComponent<WireRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        PlaceMeasurements();
    }

    void PlaceMeasurements()
    {
        
        // Erase last information
        for (int i = 0; i < _createdInformation.Count; i++)
        {
            Destroy(_createdInformation[i]);
        }
        _createdInformation.Clear();
        _localWireRenderer.EraseRange(0, _localWireRenderer.Positions.Count);
        if (_wireUserCreator.SegmentList.Count == 0)
        {
            return;
        }

        // Create line visual
        for (int i = 0; i < _wireUserRenderer.Positions.Count; i++)
        {
            (Vector3 pos, Quaternion rot) = _wireUserRenderer.GetPositionRotation(i);

            pos += WireRenderer.GetUp(pos, rot) * LocalRendererOffset();
            
            _localWireRenderer.AddPositionRotation(pos, rot);
        }
        
        Debug.Log("Started generation");

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
        
        _localWireRenderer.SetSubmesh(coloredPositions, 1);
    }

    private float LocalRendererOffset()
    {
        return _wireUserRenderer.Radius + _localWireRenderer.Radius + lineOffsetFromWire;
    }

    private float InformationOffset()
    {
        return LocalRendererOffset() + _localWireRenderer.Radius;
    }

    private void GenerateLineInfo(int startIndex, int endindex)
    {
        (Vector3 start, Quaternion startRot) = _wireUserRenderer.GetPositionRotation(startIndex);
        Vector3 end = _wireUserRenderer.Positions[endindex];
        
        Vector3 middle = Vector3.Lerp(end,start, 0.5f);
        middle += WireRenderer.GetUp(start, startRot) * InformationOffset();

        GameObject go = Instantiate(informationPrefab, middle, Quaternion.identity, transform);
        go.GetComponent<WireInfoMediator>().SetLine(Vector3.Distance(end, start));
        _createdInformation.Add(go);
        
        Debug.Log(middle);
    }

    private void GenerateCurveInfo(Curve curve)
    {
        float curvatureFlip = 1.0f;
        float curvatureDegrees = curve.CurvatureAngleDegrees;
        if (curvatureDegrees < 0)
        {
            curvatureFlip = -1.0f;
            curvatureDegrees *= -1.0f;
        }

        curvatureFlip *= -1.0f;

        float angle = curvatureDegrees / 2.0f;
        const float DIST_FROM_CENTER = 1.5f;
        
        // Get the pivot point
        (Vector3 startPoint, Quaternion startRotation) = _wireUserRenderer.GetPositionRotation(curve.StartPointIndex);
        Vector3 startForward = WireRenderer.GetForward(startPoint, startRotation);
        // The original pivot direction
        Vector3 pivotDirection = WireRenderer.GetRight(startPoint, startRotation) * curvatureFlip;
        pivotDirection = Quaternion.AngleAxis(curve.AngleTwistDegrees, startForward) * pivotDirection * DIST_FROM_CENTER;
        Vector3 pivotPoint = pivotDirection + startPoint;
        
        // Get the rotation that must be completed around the pivot
        Vector3 rotationAxis = Vector3.Cross(startForward, pivotDirection.normalized);
        Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
        
        Vector3 finalPoint = rotation * (startPoint - pivotPoint) + pivotPoint;
        Quaternion finalRotation = rotation * startRotation;
        
        finalPoint += WireRenderer.GetUp(finalPoint, finalRotation) * InformationOffset();
        
        Debug.Log(finalPoint);
        
        GameObject go = Instantiate(informationPrefab, finalPoint, Quaternion.identity, transform);
        go.GetComponent<WireInfoMediator>().SetCurve(DIST_FROM_CENTER, curve.AngleTwistDegrees, curve.CurvatureAngleDegrees);
        _createdInformation.Add(go);
    }
}
