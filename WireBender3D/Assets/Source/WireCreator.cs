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
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Add curve
            Vector3 up = _wireRenderer.GetUp(lastPos, lastRot);
            Segment newSegment = CreateCurve(lastPos + up * 1.5f, 32);
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
        
        
    }

    public void AddDebugCurve()
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 up = _wireRenderer.GetUp(lastPos, lastRot);
        CreateCurve(lastPos + up * 1.5f, 90);
    }

    public void AddLine(float length = 1.0f)
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
    }

    private Segment CreateCurve(Vector3 pivotPoint, float angleDegrees)
    {
        const int N_STEPS = 8;
        const float DIST_FROM_CENTER = 1.5f;
        float angleStep = angleDegrees / N_STEPS;
        Segment segment = new Segment(_wireRenderer.positions.Count - 1, _wireRenderer.positions.Count + N_STEPS - 1);
        
        (Vector3 startPoint, Quaternion startRotation) = _wireRenderer.GetLastPositionRotation();
        for (int i = 0; i < N_STEPS; i++)
        {
            Vector3 pivotVector = startPoint - pivotPoint;

            Vector3 rotationAxis = Vector3.Cross(pivotVector.normalized, _wireRenderer.GetForward(startPoint, startRotation));
            Quaternion rotation = Quaternion.AngleAxis(angleStep, rotationAxis);

            Vector3 finalPoint = rotation * pivotVector + pivotPoint;
            Quaternion finalRotation = startRotation * rotation;
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
        _wireRenderer.positions.RemoveRange(start, count);
        _wireRenderer.orientations.RemoveRange(start, count);
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

        if (GUILayout.Button("Quick add point"))
        {
            wireCreator.AddDebugCurve();
        }
    }
}

#endif
