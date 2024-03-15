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

    [Tooltip("The angle in a curvature before creating a step. This will influence the \"resolution\" of the curve")]
    [Min(1e-6f)]
    [SerializeField] 
    private float _curveAngleStep = 5.0f;
    
    
    private WireRenderer _wireRenderer;
    private List<Curve> _segmentList = new List<Curve>() {new Curve(0, 1), new Curve(1, 2)};
    private int selectedSegment = -1;
    
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
            selectedSegment = _segmentList.Count - 1;
        }
        
        if (_segmentList.Count == 0 || selectedSegment == -1)
        {
            return;
        }
        
        Curve currentSegment = _segmentList[selectedSegment];
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // erase segment
            EraseSegment(selectedSegment);
            if (selectedSegment >= _segmentList.Count)
            {
                selectedSegment = _segmentList.Count - 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            // rotate
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.PivotAngleDegrees = (newSegmentData.PivotAngleDegrees + 15.0f) % 360.0f;
            ReplaceSegment(selectedSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            // counter-rotate
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.PivotAngleDegrees = (newSegmentData.PivotAngleDegrees - 15.0f) % 360.0f;
            ReplaceSegment(selectedSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            // extend curvature
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.AngleDegrees = Mathf.Clamp(newSegmentData.AngleDegrees + 15.0f, 0.0f, 360.0f);
            ReplaceSegment(selectedSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // retract curvature
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.AngleDegrees = Mathf.Clamp(newSegmentData.AngleDegrees - 15.0f, 0.0f, 360.0f);
            ReplaceSegment(selectedSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            // extend line
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.DistanceFromEnd = Mathf.Max(0.0f, newSegmentData.DistanceFromEnd + 0.1f);
            ReplaceSegment(selectedSegment, newSegmentData);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            // retract line
            CurveData newSegmentData = currentSegment.CurveData.Clone();
            newSegmentData.DistanceFromEnd = Mathf.Max(0.0f, newSegmentData.DistanceFromEnd - 0.1f);
            ReplaceSegment(selectedSegment, newSegmentData);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedSegment = Mathf.Min(_segmentList.Count, selectedSegment + 1);;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedSegment = Mathf.Max(0, selectedSegment - 1);
        }
        
        
    }
    
    private void AddNewSegment()
    {
        CurveData curveData = new CurveData(0, 90.0f, 1.0f);
        InsertNewSegment(curveData, _segmentList.Count, _wireRenderer.GetPositionsCount());
    }
    
    private void ReplaceSegment(int oldSegmentIndex, CurveData newCurveData)
    {
        int pointInsertionIndex = _segmentList[oldSegmentIndex].StartPointIndex + 1;
        Debug.Log("insert index: " + pointInsertionIndex);
        EraseSegment(oldSegmentIndex);
        InsertNewSegment(newCurveData, oldSegmentIndex, pointInsertionIndex);
    }

    private void InsertNewSegment(CurveData curveData, int segmentInsertionIndex, int pointInsertionIndex)
    {
        int startIndex = pointInsertionIndex - 1;
        int totalPoints = CreateCurve(curveData.PivotAngleDegrees, curveData.AngleDegrees, pointInsertionIndex);
        totalPoints += CreateLine(curveData.DistanceFromEnd, pointInsertionIndex + totalPoints);
        int endIndex = startIndex + totalPoints;
        Curve newSegment = new Curve(startIndex, endIndex, curveData);
        _segmentList.Insert(segmentInsertionIndex, newSegment);
    }
    
    private void EraseSegment(int segmentIndex)
    {
        Curve segment = _segmentList[segmentIndex];
        _segmentList.RemoveAt(segmentIndex);
        int start = segment.StartPointIndex + 1;
        int count = segment.EndPointIndex - segment.StartPointIndex;
        _wireRenderer.EraseRange(start, count);
    }

    public void AddDebugCurve()
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 up = WireRenderer.GetUp(lastPos, lastRot);
        CreateCurve(0, 90, _wireRenderer.GetPositionsCount());
    }

    public int CreateLine(float length, int insertionIndex)
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetPositionRotation(insertionIndex - 1);
        Vector3 forward = WireRenderer.GetForward(lastPos, lastRot);
        _wireRenderer.InsertPositionRotation(lastPos + forward * length, lastRot, insertionIndex);

        return 1;
    }

    private int CreateCurve(float pivotAngleDegrees, float curvatureDegrees, int insertionIndex)
    {
        int nSteps = Mathf.Max(1, (int) (curvatureDegrees / _curveAngleStep));
        float angleStep = curvatureDegrees / nSteps;
        const float DIST_FROM_CENTER = 1.5f;
        
        // Get the pivot point
        (Vector3 startPoint, Quaternion startRotation) = _wireRenderer.GetPositionRotation(insertionIndex - 1);
        Vector3 startForward = WireRenderer.GetForward(startPoint, startRotation);
        Vector3 pivotDirection = WireRenderer.GetUp(startPoint, startRotation);
        pivotDirection = Quaternion.AngleAxis(pivotAngleDegrees, startForward) * pivotDirection * DIST_FROM_CENTER;
        Vector3 pivotPoint = pivotDirection + startPoint;
        
        // Get the rotation that must be completed around the pivot
        Vector3 rotationAxis = Vector3.Cross(startForward, pivotDirection.normalized);
        Quaternion rotation = Quaternion.AngleAxis(angleStep, rotationAxis);
        
        // The curved points
        for (int i = 0; i < nSteps; i++)
        {
            // Rotate the current point around the pivot
            Vector3 finalPoint = rotation * (startPoint - pivotPoint) + pivotPoint;
            Quaternion finalRotation = rotation * startRotation;
            _wireRenderer.InsertPositionRotation(finalPoint, finalRotation, insertionIndex + i);

            startPoint = finalPoint;
            startRotation = finalRotation;
        }

        return nSteps;
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
