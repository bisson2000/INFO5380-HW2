using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WireRenderer))]
[ExecuteInEditMode]
public class WireCreator : MonoBehaviour
{
    
    /* Strategy:
     *
     * 3 options for the user: Create a line/ Create a curve / Create arc
     *
     * A line has a start point and an end point
     * A curve has a Start point, end point, and intermediary points 
     * An arc is like many little curves
     *
     * When creating a line, you basically add a single point in the same direction as the last point
     *
     * ---- Curve
     * The curve will always be a Torus
     * When creating a curve, it is more complex.
     * Between the start point and the end point, there are other points.
     *
     * Let's say we have 2 control points, the start and the finish. Those control points cannot be extended
     * 
     */
    
    private WireRenderer _wireRenderer;
    private List<Curve> _segmentList = new List<Curve>() {new Curve(0, 1), new Curve(1, 2)};
    
    // Start is called before the first frame update
    void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Add curve
            AddNewSegment();
        }
        
        if (_segmentList.Count == 0)
        {
            return;
        }
        
        Curve currentSegment = _segmentList[^1];
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // erase segment
            EraseSegment(currentSegment);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            // rotate
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.PivotAngleDegrees = (newSegmentData.PivotAngleDegrees + 15.0f) % 360.0f;
            ReplaceSegment(currentSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            // counter-rotate
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.PivotAngleDegrees = (newSegmentData.PivotAngleDegrees - 15.0f) % 360.0f;
            ReplaceSegment(currentSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            // extend curvature
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.AngleDegrees = Mathf.Clamp(newSegmentData.AngleDegrees + 15.0f, 0.0f, 360.0f);
            ReplaceSegment(currentSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // retract curvature
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.AngleDegrees = Mathf.Clamp(newSegmentData.AngleDegrees - 15.0f, 0.0f, 360.0f);
            ReplaceSegment(currentSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            // extend line
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.DistanceFromEnd = Mathf.Max(0.0f, newSegmentData.DistanceFromEnd + 0.1f);
            ReplaceSegment(currentSegment, newSegmentData);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            // retract line
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.DistanceFromEnd = Mathf.Max(0.0f, newSegmentData.DistanceFromEnd - 0.1f);
            ReplaceSegment(currentSegment, newSegmentData);
        }
        
        
    }
    
    private void AddNewSegment()
    {
        CurveData curveData = new CurveData(0, 45.0f, 1.0f);
        AddNewSegment(curveData);
    }

    private void AddNewSegment(CurveData curveData)
    {
        int startIndex = _wireRenderer.GetPositionsCount() - 1;
        int endIndex = _wireRenderer.GetPositionsCount() - 1 + 8 + 1; // TODO: get this elsewhere
        Curve newSegment = new Curve(startIndex, endIndex, curveData);
        CreateCurve(curveData.PivotAngleDegrees, curveData.AngleDegrees);
        CreateLine(curveData.DistanceFromEnd);
        _segmentList.Add(newSegment);
    }

    private void ReplaceSegment(Curve oldSegment, CurveData newCurveData)
    {
        EraseSegment(oldSegment);
        AddNewSegment(newCurveData);
    }
    
    private void EraseSegment(Curve curve)
    {
        _segmentList.RemoveAt(_segmentList.Count - 1);
        int start = curve.StartPointIndex + 1;
        int count = curve.EndPointIndex - curve.StartPointIndex;
        _wireRenderer.EraseRange(start, count);
    }

    private float getDistance(int start, int end)
    {
        return Vector3.Distance(_wireRenderer.Positions[start], _wireRenderer.Positions[end]);
    }

    public void AddDebugCurve()
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 up = WireRenderer.GetUp(lastPos, lastRot);
        CreateCurve(0, 90);
    }

    public void CreateLine(float length)
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 forward = WireRenderer.GetForward(lastPos, lastRot);
        _wireRenderer.AddPositionRotation(lastPos + forward * length, lastRot);
    }

    private void CreateCurve(float pivotAngleDegrees, float curvatureDegrees, float startOffset = 0.0f)
    {
        const int N_STEPS = 8;
        const float DIST_FROM_CENTER = 1.5f;
        float angleStep = curvatureDegrees / N_STEPS;
        //int startIndex = _wireRenderer.GetPositionsCount() - 1;
        // TODO: Check if we should add one at the end as well. most likely not
        // +1 for the start point
        //int endIndex = _wireRenderer.GetPositionsCount() - 1 + N_STEPS + 1; 
        //Curve segment = new Curve(startIndex, endIndex, pivotAngleDegrees, angleDegrees);
        
        // Get the pivot point
        (Vector3 startPoint, Quaternion startRotation) = _wireRenderer.GetLastPositionRotation();
        Vector3 startForward = WireRenderer.GetForward(startPoint, startRotation);
        Vector3 pivotDirection = WireRenderer.GetUp(startPoint, startRotation);
        pivotDirection = Quaternion.AngleAxis(pivotAngleDegrees, startForward) * pivotDirection * DIST_FROM_CENTER;
        Vector3 pivotPoint = pivotDirection + startPoint;
        
        // Get the rotation that must be completed around the pivot
        Vector3 rotationAxis = Vector3.Cross(startForward, pivotDirection.normalized);
        Quaternion rotation = Quaternion.AngleAxis(angleStep, rotationAxis);

        // The start point, aligned
        //_wireRenderer.AddPositionRotation(startPoint + startForward * startOffset, startRotation);
        
        // The curved points
        for (int i = 0; i < N_STEPS; i++)
        {
            // Rotate the current point around the pivot
            Vector3 finalPoint = rotation * (startPoint - pivotPoint) + pivotPoint;
            Quaternion finalRotation = rotation * startRotation;
            _wireRenderer.AddPositionRotation(finalPoint, finalRotation);

            startPoint = finalPoint;
            startRotation = finalRotation;
        }

        //return segment;
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(WireCreator))]
public class WirCreatorEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        WireCreator wireCreator = target as WireCreator;
        if (wireCreator == null)
        {
            return;
        }

        if (GUILayout.Button("Quick add curve"))
        {
            wireCreator.AddDebugCurve();
        }
    }
}

#endif