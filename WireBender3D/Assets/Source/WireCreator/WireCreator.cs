using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WireRenderer))]
[ExecuteInEditMode]
public class WireCreator : MonoBehaviour
{
    [Tooltip("The angle in a curvature before creating a step. This will influence the \"resolution\" of the curve")]
    [Min(1e-6f)]
    [SerializeField] 
    private float _curveAngleStep = 15.0f;
    
    
    private WireRenderer _wireRenderer;
    private List<Segment> _segmentList = new List<Segment>() {new Line(0, 1, 0, 1), new Line(1, 2, 0, 1)};
    private int _selectedSegment = -1;
    private Dictionary<KeyCode, float> lastActionTime = new Dictionary<KeyCode, float>();    
    // Start is called before the first frame update
    void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
        InitializeActionTiming();  // Initialize action timing for all relevant keys

        for (int i = 0; i < _wireRenderer.Positions.Count - 1; i++)
        {
            float length = Vector3.Distance(_wireRenderer.Positions[i + 1], _wireRenderer.Positions[i]);
            Line newLine = new Line(i, i + 1, 0.0f, length);
            _segmentList.Add(newLine);
        }
    }
    
    private void InitializeActionTiming()
    {
        KeyCode[] keys = {
            KeyCode.A, KeyCode.Q, KeyCode.Z, KeyCode.R, KeyCode.T, 
            KeyCode.F, KeyCode.G, KeyCode.V, KeyCode.B, KeyCode.UpArrow, KeyCode.DownArrow
        };
        foreach (KeyCode key in keys)
        {
            lastActionTime[key] = -Mathf.Infinity;
        }
    }

    private bool CanPerformAction(KeyCode key)
    {
        const float actionInterval = 0.1f; // For 10 actions per second
        float currentTime = Time.time;

        if (currentTime - lastActionTime[key] >= actionInterval)
        {
            lastActionTime[key] = currentTime;
            return true;
        }
        return false;
    }

    void Update()
    {
        HandleKeyboardInput();
    }
    
    // Function that holds keyboard key functions
    private void HandleKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return; // Exit if no keyboard is detected

        
        // Key Q: Add a new curve segment
        if (keyboard.qKey.isPressed && CanPerformAction(KeyCode.Q))
        {
            AddNewCurve(); // This method handles the logic to add a new curve segment
        }
        
        // Key A: Add a new straight segment
        if (keyboard.aKey.isPressed && CanPerformAction(KeyCode.A))
        {
            // Add line
            AddNewLine(); // This method handles the logic to add a new straight line segment
        }

        // Exit early if no segments are selected or available
        if (_segmentList.Count == 0 || _selectedSegment == -1) return;

        // Retrieve the current segment based on the selected index
        Segment currentSegment = _segmentList[_selectedSegment];

        // Key Z: Erase the current segment
        if (keyboard.zKey.isPressed && CanPerformAction(KeyCode.Z))
        {
            RemoveSegmentAndPropagate(_selectedSegment);
            SetSelectedSegment(_selectedSegment - 1); // Update selection
        }

        // Key R: Rotate the current segment clockwise
        if (keyboard.rKey.isPressed && CanPerformAction(KeyCode.R))
        {
            Segment newSegmentData = currentSegment.Clone();;
            newSegmentData.AngleTwistDegrees = (newSegmentData.AngleTwistDegrees + 15.0f) % 360.0f;
            ReplaceSegment(_selectedSegment, newSegmentData);
        }

        // Key T: Rotate the current segment counter-clockwise
        if (keyboard.tKey.isPressed && CanPerformAction(KeyCode.T))
        {
            Segment newSegmentData = currentSegment.Clone();;
            newSegmentData.AngleTwistDegrees = (newSegmentData.AngleTwistDegrees - 15.0f) % 360.0f;
            ReplaceSegment(_selectedSegment, newSegmentData);
        }

        // Key F: Extend the curvature of the current segment
        if (keyboard.fKey.isPressed && CanPerformAction(KeyCode.F))
        {
            // extend curvature
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Curve curve)
            {
                curve.CurvatureAngleDegrees = (curve.CurvatureAngleDegrees + 15.0f) % 360.0f;
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
        }

        // Key G: Retract the curvature of the current segment
        if (keyboard.gKey.isPressed && CanPerformAction(KeyCode.G))
        {
            // retract curvature
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Curve curve)
            {
                curve.CurvatureAngleDegrees = (curve.CurvatureAngleDegrees - 15.0f) % 360.0f;
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
        }

        // Key V: Extend the length of the current segment
        if (keyboard.vKey.isPressed && CanPerformAction(KeyCode.V))
        {
            // extend line
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Line line)
            {
                line.Length = Mathf.Max(0.0f, line.Length + 0.1f);
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
        }

        // Key B: Retract the length of the current segment
        if (keyboard.bKey.isPressed && CanPerformAction(KeyCode.B))
        {
            // retract line
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Line line)
            {
                line.Length = Mathf.Max(0.0f, line.Length - 0.1f);
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
        }

        // Key UpArrow: Select the next segment
        if (keyboard.upArrowKey.isPressed && CanPerformAction(KeyCode.UpArrow))
        {
            SetSelectedSegment(_selectedSegment + 1); // Moves the selection to the next segment
        }

        // Key DownArrow: Select the previous segment
        if (keyboard.downArrowKey.isPressed && CanPerformAction(KeyCode.DownArrow))
        {
            SetSelectedSegment(Mathf.Max(0, _selectedSegment - 1)); // Moves the selection to the previous segment, ensuring it doesn't go below 0
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetSelectedSegment(_selectedSegment + 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetSelectedSegment(_selectedSegment - 1);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            string builder = "";
            foreach (Vector3 point in _wireRenderer.Positions)
            {
                builder += "[" + point.x.ToString("F", CultureInfo.InvariantCulture) + "," + point.y.ToString("F", CultureInfo.InvariantCulture) + "," + point.z.ToString("F", CultureInfo.InvariantCulture) + "],\n";
            }

            Debug.Log(builder);
        }
    }
    
    /// <summary>
    /// Sets the selected segment to the passed index. Will also set the submesh for the wire renderer.
    /// </summary>
    /// <param name="selectedIndex">The index of the segment to select</param>
    private void SetSelectedSegment(int selectedIndex)
    {
        if (_segmentList.Count == 0)
        {
            _selectedSegment = -1;
            _wireRenderer.SetSubmesh(-1,  0);
            return;
        }
        
        _selectedSegment = Mathf.Clamp(selectedIndex, 0, _segmentList.Count - 1);
        int start = _segmentList[_selectedSegment].StartPointIndex;
        int count = _segmentList[_selectedSegment].EndPointIndex - start;
        _wireRenderer.SetSubmesh(start,  count);
    }
    
    /// <summary>
    /// Adds a new curve after the current selection index.
    /// </summary>
    private void AddNewCurve()
    {
        int segmentInsertionIndex = _segmentList.Count;
        int pointInsertionIndex = _wireRenderer.GetPositionsCount();
        if (0 <= _selectedSegment && _selectedSegment <  _segmentList.Count)
        {
            segmentInsertionIndex = _selectedSegment + 1;
            pointInsertionIndex = _segmentList[_selectedSegment].EndPointIndex + 1;
        }
        
        const float twistDegrees = 0.0f;
        InsertNewCurve(segmentInsertionIndex, pointInsertionIndex, twistDegrees, 90.0f);
        
        PropagateChange(twistDegrees, segmentInsertionIndex + 1, false);
        
        SetSelectedSegment(segmentInsertionIndex);
    }
    
    /// <summary>
    /// Adds a new line after the current selection index.
    /// </summary>
    private void AddNewLine()
    {
        int segmentInsertionIndex = _segmentList.Count;
        int pointInsertionIndex = _wireRenderer.GetPositionsCount();
        if (0 <= _selectedSegment && _selectedSegment <  _segmentList.Count)
        {
            segmentInsertionIndex = _selectedSegment + 1;
            pointInsertionIndex = _segmentList[_selectedSegment].EndPointIndex + 1;
        }
        
        const float twistDegrees = 0.0f;
        InsertNewLine(segmentInsertionIndex, pointInsertionIndex, twistDegrees, 1.0f);
        
        PropagateChange(twistDegrees, segmentInsertionIndex + 1, false);
        
        SetSelectedSegment(segmentInsertionIndex);
    }
    
    /// <summary>
    /// Replaces an old segment with a new one.
    /// </summary>
    /// <param name="oldSegmentIndex">The segment index representing the old segment</param>
    /// <param name="newSegmentData">The new segment to replace it with</param>
    private void ReplaceSegment(int oldSegmentIndex, Segment newSegmentData)
    {
        // save change
        float twistChange = newSegmentData.AngleTwistDegrees - _segmentList[oldSegmentIndex].AngleTwistDegrees;
        
        // Apply the change
        EraseSegment(oldSegmentIndex);
        InsertNewSegment(oldSegmentIndex, newSegmentData.StartPointIndex + 1, newSegmentData);
        
        // Propagate the change
        PropagateChange(twistChange, oldSegmentIndex + 1, false);
        
        // Update selection
        SetSelectedSegment(_selectedSegment);
    }
    
    /// <summary>
    /// Removes the segment at the specified index and propagate the changes.
    /// </summary>
    /// <param name="segmentIndex"></param>
    private void RemoveSegmentAndPropagate(int segmentIndex)
    {
        float twistChange = _segmentList[segmentIndex].AngleTwistDegrees * -1.0f;

        EraseSegment(segmentIndex);
        PropagateChange(twistChange, segmentIndex, true);
    }
    
    /// <summary>
    /// Propagate changes from one segment to the next ones.
    /// </summary>
    /// <param name="twistDegrees">The change to propagate</param>
    /// <param name="nextSegmentIndex">The next segment index to start propagating the change to</param>
    /// <param name="unfoldStyle">The style how the change is propagated. By setting to true, it will apply changes
    /// the same way as if the curves were "unfolded" and then refolded. By setting to false, the curves will
    /// be modified by the changes</param>
    private void PropagateChange(float twistDegrees, int nextSegmentIndex, bool unfoldStyle)
    {
        for (int i = nextSegmentIndex; i < _segmentList.Count; ++i)
        {
            int newStartIndex = 0;
            if (i > 0)
            {
                newStartIndex = _segmentList[i - 1].EndPointIndex;
            }
            
            
            int newEndIndex = _segmentList[i].EndPointIndex + newStartIndex - _segmentList[i].StartPointIndex;
            
            _segmentList[i].StartPointIndex = newStartIndex;
            _segmentList[i].EndPointIndex = newEndIndex;
            Segment data = _segmentList[i].Clone();
            if (!unfoldStyle)
            {
                data.AngleTwistDegrees += twistDegrees;
            }
            
            EraseSegment(i);
            InsertNewSegment(i, newStartIndex + 1, data);
        }
    }

    /// <summary>
    /// Inserts a segment at the desired location
    /// </summary>
    /// <param name="segmentInsertionIndex">The segment index to insert the segment to</param>
    /// <param name="pointInsertionIndex">The point used by WireRenderer to insert the segment to</param>
    /// <param name="segment">The segment to insert</param>
    private void InsertNewSegment(int segmentInsertionIndex, int pointInsertionIndex, Segment segment)
    {
        if (segment is Curve curve)
        {
            InsertNewCurve(segmentInsertionIndex, pointInsertionIndex, segment.AngleTwistDegrees, curve.CurvatureAngleDegrees);
        }
        else if (segment is Line line)
        {
            InsertNewLine(segmentInsertionIndex, pointInsertionIndex, segment.AngleTwistDegrees, line.Length);
        }
    }
    
    /// <summary>
    /// Inserts a curve at the desired location
    /// </summary>
    /// <param name="segmentInsertionIndex">The segment index to insert the segment to</param>
    /// <param name="pointInsertionIndex">The point used by WireRenderer to insert the segment to</param>
    /// <param name="twistDegrees"></param>
    /// <param name="curvatureAngleDegrees"></param>
    private void InsertNewCurve(int segmentInsertionIndex, int pointInsertionIndex, float twistDegrees, float curvatureAngleDegrees)
    {
        int startIndex = pointInsertionIndex - 1;
        int totalPoints = CreateCurve(pointInsertionIndex, twistDegrees, curvatureAngleDegrees);
        int endIndex = startIndex + totalPoints;
        Curve newSegment = new Curve(startIndex, endIndex, twistDegrees, curvatureAngleDegrees);
        _segmentList.Insert(segmentInsertionIndex, newSegment);
    }
    
    /// <summary>
    /// Inserts a line at the desired location
    /// </summary>
    /// <param name="segmentInsertionIndex">The segment index to insert the segment to</param>
    /// <param name="pointInsertionIndex">The point used by WireRenderer to insert the segment to</param>
    /// <param name="twistDegrees"></param>
    /// <param name="length"></param>
    private void InsertNewLine(int segmentInsertionIndex, int pointInsertionIndex, float twistDegrees, float length)
    {
        int startIndex = pointInsertionIndex - 1;
        int totalPoints = CreateLine(pointInsertionIndex, length);
        int endIndex = startIndex + totalPoints;
        Line newSegment = new Line(startIndex, endIndex, twistDegrees, length);
        _segmentList.Insert(segmentInsertionIndex, newSegment);
    }
    
    /// <summary>
    /// Erase a segment at the desired index.
    /// </summary>
    /// <param name="segmentIndex">Index of the segment to erase</param>
    private void EraseSegment(int segmentIndex)
    {
        Segment segment = _segmentList[segmentIndex];
        _segmentList.RemoveAt(segmentIndex);
        int start = segment.StartPointIndex + 1;
        int count = segment.EndPointIndex - segment.StartPointIndex;
        _wireRenderer.EraseRange(start, count);
    }

    /// <summary>
    /// Create a line in the WireRenderer
    /// </summary>
    /// <param name="insertionIndex">The position in WireRenderer to insert to</param>
    /// <param name="length">The length of the line</param>
    /// <returns>The number of points created</returns>
    public int CreateLine(int insertionIndex, float length)
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetPositionRotation(insertionIndex - 1);
        Vector3 forward = WireRenderer.GetForward(lastPos, lastRot);
        
        _wireRenderer.InsertPositionRotation(lastPos + forward * length, lastRot, insertionIndex);

        return 1;
    }

    /// <summary>
    /// Create a curve in the WireRenderer
    /// </summary>
    /// <param name="insertionIndex">The position in WireRenderer to insert to</param>
    /// <param name="pivotAngleDegrees">The rotation around itself</param>
    /// <param name="curvatureDegrees">The curvature</param>
    /// <returns>The number of points created</returns>
    private int CreateCurve(int insertionIndex, float pivotAngleDegrees, float curvatureDegrees)
    {
        float curvatureFlip = 1.0f;
        if (curvatureDegrees < 0)
        {
            curvatureFlip = -1.0f;
            curvatureDegrees *= -1.0f;
        }
        
        int nSteps = Mathf.Max(1, (int) (curvatureDegrees / _curveAngleStep));
        float angleStep = curvatureDegrees / nSteps;
        const float DIST_FROM_CENTER = 1.5f;
        
        // Get the pivot point
        (Vector3 startPoint, Quaternion startRotation) = _wireRenderer.GetPositionRotation(insertionIndex - 1);
        Vector3 startForward = WireRenderer.GetForward(startPoint, startRotation);
        Vector3 pivotDirection = WireRenderer.GetUp(startPoint, startRotation) * curvatureFlip;
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
    
    public void AddDebugCurve()
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 up = WireRenderer.GetUp(lastPos, lastRot);
        CreateCurve(_wireRenderer.GetPositionsCount(), 0, 90);
    }
    
    public void AddDebugLine()
    {
        (Vector3 lastPos, Quaternion lastRot) = _wireRenderer.GetLastPositionRotation();
        Vector3 up = WireRenderer.GetUp(lastPos, lastRot);
        CreateLine(_wireRenderer.GetPositionsCount(), 1);
    }
    
}


#if UNITY_EDITOR

[CustomEditor(typeof(WireCreator))]
public class WireCreatorEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        WireCreator wireCreator = target as WireCreator;
        if (wireCreator == null)
        {
            return;
        }
        
        if (GUILayout.Button("Quick add line"))
        {
            wireCreator.AddDebugLine();
        }

        if (GUILayout.Button("Quick add curve"))
        {
            wireCreator.AddDebugCurve();
        }
    }
}

#endif