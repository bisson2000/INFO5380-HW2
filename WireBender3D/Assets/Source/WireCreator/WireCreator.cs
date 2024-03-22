using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(WireRenderer))]
public class WireCreator : MonoBehaviour
{
    [Tooltip("The angle in a curvature before creating a step. This will influence the \"resolution\" of the curve")]
    [Min(1e-6f)]
    [SerializeField] 
    private float _curveAngleStep = 15.0f;
    public float CurveAngleStep => _curveAngleStep;
    
    protected WireRenderer _wireRenderer;
    protected List<Segment> _segmentList = new List<Segment>();
    public IReadOnlyList<Segment> SegmentList => _segmentList.AsReadOnly();
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
        for (int i = 0; i < _wireRenderer.Positions.Count - 1; i++)
        {
            float length = Vector3.Distance(_wireRenderer.Positions[i + 1], _wireRenderer.Positions[i]);
            Line newLine = new Line(i, i + 1, length);
            _segmentList.Add(newLine);
        }
    }

    /*public void SetSegments(List<Segment> segments)
    {
        // Erase everything
        for (int i = _segmentList.Count - 1; i >= 0; i--)
        {
            EraseSegment(i);
        }
        
        // Set all segments
        for (int i = 0; i < segments.Count; i++)
        {
            
            InsertNewSegment(i, segments[i].StartPointIndex + 1, segments[i]);
        }
        
    }*/
    
    /// <summary>
    /// Replaces an old segment with a new one.
    /// </summary>
    /// <param name="oldSegmentIndex">The segment index representing the old segment</param>
    /// <param name="newSegmentData">The new segment to replace it with</param>
    /// <param name="twistChange">The twist change</param>
    protected virtual void ReplaceSegment(int oldSegmentIndex, Segment newSegmentData, float twistChange)
    {
         // Apply the change
        EraseSegment(oldSegmentIndex);
        InsertNewSegment(oldSegmentIndex, newSegmentData.StartPointIndex + 1, newSegmentData);
        
        // Propagate the change
        PropagateChange(twistChange, oldSegmentIndex + 1, false);
    }
    
    /// <summary>
    /// Removes the segment at the specified index and propagate the changes.
    /// </summary>
    /// <param name="segmentIndex"></param>
    protected void RemoveSegmentAndPropagate(int segmentIndex)
    {
        float twistChange = 0.0f;
        if (_segmentList[segmentIndex] is Curve curve)
        {
            twistChange = curve.AngleTwistDegrees * -1.0f;
        }

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
    protected void PropagateChange(float twistDegrees, int nextSegmentIndex, bool unfoldStyle)
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
            if (!unfoldStyle && data is Curve curve)
            {
                curve.AngleTwistDegrees = (curve.AngleTwistDegrees + twistDegrees) % 360.0f;
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
    protected void InsertNewSegment(int segmentInsertionIndex, int pointInsertionIndex, Segment segment)
    {
        if (segment is Curve curve)
        {
            InsertNewCurve(segmentInsertionIndex, pointInsertionIndex, curve.AngleTwistDegrees, curve.CurvatureAngleDegrees);
        }
        else if (segment is Line line)
        {
            InsertNewLine(segmentInsertionIndex, pointInsertionIndex, line.Length);
        }
    }
    
    /// <summary>
    /// Inserts a curve at the desired location
    /// </summary>
    /// <param name="segmentInsertionIndex">The segment index to insert the segment to</param>
    /// <param name="pointInsertionIndex">The point used by WireRenderer to insert the segment to</param>
    /// <param name="twistDegrees"></param>
    /// <param name="curvatureAngleDegrees"></param>
    protected void InsertNewCurve(int segmentInsertionIndex, int pointInsertionIndex, float twistDegrees, float curvatureAngleDegrees)
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
    protected void InsertNewLine(int segmentInsertionIndex, int pointInsertionIndex, float length)
    {
        int startIndex = pointInsertionIndex - 1;
        int totalPoints = CreateLine(pointInsertionIndex, length);
        int endIndex = startIndex + totalPoints;
        Line newSegment = new Line(startIndex, endIndex, length);
        _segmentList.Insert(segmentInsertionIndex, newSegment);
    }
    
    /// <summary>
    /// Erase a segment at the desired index.
    /// </summary>
    /// <param name="segmentIndex">Index of the segment to erase</param>
    protected void EraseSegment(int segmentIndex)
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
