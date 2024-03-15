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
    private List<Segment> _segmentList = new List<Segment>() {new Segment(0, 1), new Segment(1, 2)};
    
    // Start is called before the first frame update
    void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Add curve
            Segment newSegment = CreateCurve(0, 90);
            _segmentList.Add(newSegment);
        }
        
        if (_segmentList.Count == 0)
        {
            return;
        }
        
        Segment currentSegment = _segmentList[^1];
        if (Input.GetKeyDown(KeyCode.E))
        {
            // erase segment
            EraseSegment(currentSegment);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            // rotate
            if (currentSegment is Curve currentCurve)
            {
                EraseSegment(currentCurve);
                float pivotAngleDegrees = (currentCurve.PivotAngleDegrees + 15.0f) % 360.0f;
                float angleDegrees = currentCurve.AngleDegrees;
                Segment newSegment = CreateCurve(pivotAngleDegrees, angleDegrees);
                _segmentList.Add(newSegment);
            }
            
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            // rotate
            if (currentSegment is Curve currentCurve)
            {
                EraseSegment(currentCurve);
                float pivotAngleDegrees = currentCurve.PivotAngleDegrees;
                float angleDegrees = (currentCurve.AngleDegrees + 15.0f) % 360.0f;
                Segment newSegment = CreateCurve(pivotAngleDegrees, angleDegrees);
                _segmentList.Add(newSegment);
            }
            
        }
        
        
    }

    public void AddDebugCurve()
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 up = WireRenderer.GetUp(lastPos, lastRot);
        CreateCurve(0, 90);
    }

    public void AddLine(float length = 1.0f)
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
    }

    private Segment CreateCurve(float pivotAngleDegrees, float angleDegrees)
    {
        const int N_STEPS = 8;
        const float DIST_FROM_CENTER = 1.5f;
        float angleStep = angleDegrees / N_STEPS;
        int startIndex = _wireRenderer.GetPositionsCount() - 1;
        int endIndex = _wireRenderer.GetPositionsCount() + N_STEPS - 1;
        Segment segment = new Curve(startIndex, endIndex, pivotAngleDegrees, angleDegrees);
        
        (Vector3 startPoint, Quaternion startRotation) = _wireRenderer.GetLastPositionRotation();
        Vector3 startForward = WireRenderer.GetForward(startPoint, startRotation);
        Vector3 pivotDirection = WireRenderer.GetUp(startPoint, startRotation);
        pivotDirection = Quaternion.AngleAxis(pivotAngleDegrees, startForward) * pivotDirection * DIST_FROM_CENTER;
        Vector3 pivotPoint = pivotDirection + startPoint;
        
        Vector3 rotationAxis = Vector3.Cross(startForward, pivotDirection.normalized);
        Quaternion rotation = Quaternion.AngleAxis(angleStep, rotationAxis);

        for (int i = 0; i < N_STEPS; i++)
        {

            Vector3 finalPoint = rotation * (startPoint - pivotPoint) + pivotPoint;
            Quaternion finalRotation = rotation * startRotation;
            _wireRenderer.AddPositionRotation(finalPoint, finalRotation);

            startPoint = finalPoint;
            startRotation = finalRotation;
        }

        return segment;
    }

    private void EraseSegment(Segment segment)
    {
        _segmentList.RemoveAt(_segmentList.Count - 1);
        int start = segment.StartPointIndex + 1;
        int count = segment.EndPointIndex - segment.StartPointIndex;
        _wireRenderer.EraseRange(start, count);
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
