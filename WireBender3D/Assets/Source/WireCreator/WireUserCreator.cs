using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;


public class WireUserCreator : WireCreator
{
   private int _selectedSegment = -1;

    // Update is called once per frame
    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     // Add curve
        //     AddNewCurve();
        // }
        // else if (Input.GetKeyDown(KeyCode.A))
        // {
        //     // Add line
        //     AddNewLine();
        // }
        
        // if (_segmentList.Count == 0 || _selectedSegment == -1)
        // {
        //     return;
        // }
        //
        // Segment currentSegment = _segmentList[_selectedSegment];
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     // erase segment
        //     RemoveSegmentAndPropagate(_selectedSegment);
        //     SetSelectedSegment(_selectedSegment - 1);
        // }
        // else if (Input.GetKeyDown(KeyCode.R))
        // {
        //     // rotate
        //     Segment newSegmentData = currentSegment.Clone();
        //     float twistChange = 15.0f;
        //     if (newSegmentData is Curve curve)
        //     {
        //         curve.AngleTwistDegrees = IncrementAngleDegrees(curve.AngleTwistDegrees, twistChange);
        //     }
        //     ReplaceSegment(_selectedSegment, newSegmentData, twistChange);
        //     
        // }

        
        // else if (Input.GetKeyDown(KeyCode.T))
        // {
        //     // counter-rotate
        //     Segment newSegmentData = currentSegment.Clone();
        //     float twistChange = -15.0f;
        //     if (newSegmentData is Curve curve)
        //     {
        //         curve.AngleTwistDegrees = IncrementAngleDegrees(curve.AngleTwistDegrees, twistChange);
        //     }
        //     ReplaceSegment(_selectedSegment, newSegmentData, twistChange);
        //     
        // }
        
        // else if (Input.GetKeyDown(KeyCode.F))
        // {
        //     // extend curvature
        //     Segment newSegmentData = currentSegment.Clone();
        //     if (newSegmentData is Curve curve)
        //     {
        //         float curvatureChange = 15.0f;
        //         curve.CurvatureAngleDegrees = IncrementAngleDegrees(curve.CurvatureAngleDegrees, curvatureChange);
        //         ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        //     }
        // }
        
        // else if (Input.GetKeyDown(KeyCode.G))
        // {
        //     // retract curvature
        //     Segment newSegmentData = currentSegment.Clone();
        //     if (newSegmentData is Curve curve)
        //     {
        //         float curvatureChange = -15.0f;
        //         curve.CurvatureAngleDegrees = IncrementAngleDegrees(curve.CurvatureAngleDegrees, curvatureChange);
        //         ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        //     }
        //     
        // }
        
        // else if (Input.GetKeyDown(KeyCode.V))
        // {
        //     // extend line
        //     Segment newSegmentData = currentSegment.Clone();
        //     if (newSegmentData is Line line)
        //     {
        //         line.Length = Mathf.Max(0.0f, line.Length + 0.1f);
        //         ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        //     }
        //     
        // }
        // else if (Input.GetKeyDown(KeyCode.B))
        // {
        //     // retract line
        //     Segment newSegmentData = currentSegment.Clone();
        //     if (newSegmentData is Line line)
        //     {
        //         line.Length = Mathf.Max(0.0f, line.Length - 0.1f);
        //         ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.UpArrow))
        // {
        //     SetSelectedSegment(_selectedSegment + 1);
        // }
        // else if (Input.GetKeyDown(KeyCode.DownArrow))
        // {
        //     SetSelectedSegment(_selectedSegment - 1);
        // }
        // else if (Input.GetKeyDown(KeyCode.P))
        // {
        //     string builder = "";
        //     foreach (Vector3 point in _wireRenderer.Positions)
        //     {
        //         builder += "[" + point.x.ToString("F", CultureInfo.InvariantCulture) + "," + point.y.ToString("F", CultureInfo.InvariantCulture) + "," + point.z.ToString("F", CultureInfo.InvariantCulture) + "],\n";
        //     }
        //
        //     Debug.Log(builder);
        // }
    }

    public void EraseSegment()
    {
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
    }

    public void RotateSegmentClockwise(Segment currentSegment, float addedRotation=15.0f)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        // rotate
        Segment newSegmentData = currentSegment.Clone();
        // float twistChange = 15.0f;
        if (newSegmentData is Curve curve)
        {
            curve.AngleTwistDegrees = IncrementAngleDegrees(curve.AngleTwistDegrees, addedRotation);
        }
        ReplaceSegment(_selectedSegment, newSegmentData, addedRotation);
    }
    
    public void RotateSegmentCounterClockwise(Segment currentSegment, float addedRotation=-15.0f)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        // rotate
        Segment newSegmentData = currentSegment.Clone();
        // float twistChange = -15.0f;
        if (newSegmentData is Curve curve)
        {
            curve.AngleTwistDegrees = IncrementAngleDegrees(curve.AngleTwistDegrees, addedRotation);
        }
        ReplaceSegment(_selectedSegment, newSegmentData, addedRotation);
    }

    public void ExtendCurvature(Segment currentSegment, float curvatureChange = 15.0f)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }

        // extend curvature
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Curve curve)
        {
            // float curvatureChange = 15.0f;
            curve.CurvatureAngleDegrees = IncrementAngleDegrees(curve.CurvatureAngleDegrees, curvatureChange);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        }
    }
    
    public void RetractCurvature(Segment currentSegment, float curvatureChange = -15.0f)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }

        // Retract curvature
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Curve curve)
        {
            // float curvatureChange = 15.0f;
            curve.CurvatureAngleDegrees = IncrementAngleDegrees(curve.CurvatureAngleDegrees, curvatureChange);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        }
    }

    public void ExtendLine(Segment currentSegment)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }

        // extend line
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Line line)
        {
            line.Length = Mathf.Max(0.0f, line.Length + 0.1f);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        }
    }
    
    public void RetractLine(Segment currentSegment)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }

        // Retract line
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Line line)
        {
            line.Length = Mathf.Max(0.0f, line.Length - 0.1f);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
        }
    }

    public void SelectNextSegment(Segment currentSegment)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        SetSelectedSegment(_selectedSegment + 1);
    }
    
    public void SelectPreviousSegment(Segment currentSegment)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        SetSelectedSegment(_selectedSegment - 1);
    }

    public void PrintCoordinatesAsArray(Segment currentSegment)
    {
        string builder = "";
        foreach (Vector3 point in _wireRenderer.Positions)
        {
            builder += "[" + point.x.ToString("F", CultureInfo.InvariantCulture) + "," +
                       point.y.ToString("F", CultureInfo.InvariantCulture) + "," +
                       point.z.ToString("F", CultureInfo.InvariantCulture) + "],\n";
        }

        Debug.Log(builder);
    }

    public void PrintCoordinatesAsArrayAndExportToCSV(Segment currentSegment)
    {
        // CSV content builder
        string csvContent = "";

        // Header
        csvContent += "x,y,z\n";

        // Build the CSV content
        foreach (Vector3 point in _wireRenderer.Positions)
        {
            csvContent += point.x.ToString("F", CultureInfo.InvariantCulture) + "," +
                          point.y.ToString("F", CultureInfo.InvariantCulture) + "," +
                          point.z.ToString("F", CultureInfo.InvariantCulture) + "\n";
        }

        // File path
        string filePath = Path.Combine(Application.dataPath, "Coordinates.csv");

        // Write to file
        File.WriteAllText(filePath, csvContent);

        Debug.Log("Coordinates exported to CSV at: " + filePath);
    }
    /// <summary>
    /// Sets the selected segment to the passed index. Will also set the submesh for the wire renderer.
    /// </summary>
    /// <param name="selectedIndex">The index of the segment to select</param>
    public void SetSelectedSegment(int selectedIndex)
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
    public void AddNewCurve()
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
    public void AddNewLine()
    {
        int segmentInsertionIndex = _segmentList.Count;
        int pointInsertionIndex = _wireRenderer.GetPositionsCount();
        if (0 <= _selectedSegment && _selectedSegment <  _segmentList.Count)
        {
            segmentInsertionIndex = _selectedSegment + 1;
            pointInsertionIndex = _segmentList[_selectedSegment].EndPointIndex + 1;
        }
        
        const float twistDegrees = 0.0f;
        InsertNewLine(segmentInsertionIndex, pointInsertionIndex, 1.0f);
        
        PropagateChange(twistDegrees, segmentInsertionIndex + 1, false);
        
        SetSelectedSegment(segmentInsertionIndex);
    }
    
    
    protected override void ReplaceSegment(int oldSegmentIndex, Segment newSegmentData, float twistChange)
    {
        base.ReplaceSegment(oldSegmentIndex, newSegmentData, twistChange);
        
        // Update selection
        SetSelectedSegment(_selectedSegment);
    }

    
}
