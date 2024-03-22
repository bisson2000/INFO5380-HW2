using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WireUserCreator : WireCreator
{
   private int _selectedSegment = -1;

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Add curve
            AddNewCurve();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            // Add line
            AddNewLine();
        }
        
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        
        Segment currentSegment = _segmentList[_selectedSegment];
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // erase segment
            RemoveSegmentAndPropagate(_selectedSegment);
            SetSelectedSegment(_selectedSegment - 1);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            // rotate
            Segment newSegmentData = currentSegment.Clone();;
            newSegmentData.AngleTwistDegrees = (newSegmentData.AngleTwistDegrees + 15.0f) % 360.0f;
            ReplaceSegment(_selectedSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            // counter-rotate
            Segment newSegmentData = currentSegment.Clone();;
            newSegmentData.AngleTwistDegrees = (newSegmentData.AngleTwistDegrees - 15.0f) % 360.0f;
            ReplaceSegment(_selectedSegment, newSegmentData);
            
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            // extend curvature
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Curve curve)
            {
                curve.CurvatureAngleDegrees = (curve.CurvatureAngleDegrees + 15.0f) % 360.0f;
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // retract curvature
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Curve curve)
            {
                curve.CurvatureAngleDegrees = (curve.CurvatureAngleDegrees - 15.0f) % 360.0f;
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
            
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            // extend line
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Line line)
            {
                line.Length = Mathf.Max(0.0f, line.Length + 0.1f);
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
            
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            // retract line
            Segment newSegmentData = currentSegment.Clone();;
            if (newSegmentData is Line line)
            {
                line.Length = Mathf.Max(0.0f, line.Length - 0.1f);
                ReplaceSegment(_selectedSegment, newSegmentData);
            }
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
        if (_segmentList.Count == 0 || selectedIndex < -1)
        {
            _selectedSegment = -1;
            _wireRenderer.SetSubmesh(-1,  0, 1);
            return;
        }
        
        _selectedSegment = Mathf.Clamp(selectedIndex, 0, _segmentList.Count - 1);
        int start = _segmentList[_selectedSegment].StartPointIndex;
        int count = _segmentList[_selectedSegment].EndPointIndex - start;
        _wireRenderer.SetSubmesh(start,  count, 1);
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
    protected override void ReplaceSegment(int oldSegmentIndex, Segment newSegmentData)
    {
        base.ReplaceSegment(oldSegmentIndex, newSegmentData);
        
        // Update selection
        SetSelectedSegment(_selectedSegment);
    }

    
}
