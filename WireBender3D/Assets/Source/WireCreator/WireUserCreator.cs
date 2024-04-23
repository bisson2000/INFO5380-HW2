using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;


public class WireUserCreator : WireCreator
{
    public Action OnChange;
    public Action OnSelectionChange;
    private int _selectedSegment = -1;
    public int SelectedSegment => _selectedSegment;
    
    /// <summary>
    /// Delete all segments.   
    /// </summary>
    public void EraseAllSegments()
    {
        if (_segmentList.Count == 0)
        {
            return;
        }
        
        SetSelectedSegment(-2);
        for (int i = _segmentList.Count - 1; i >= 0; i--)
        {
            RemoveSegmentAndPropagate(i);
        }
        
        OnChange?.Invoke();
    }
   
    /// <summary>
    /// Deletes selected segment.   
    /// </summary>
    public void EraseSegment()
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        // erase segment
        RemoveSegmentAndPropagate(_selectedSegment);
        SetSelectedSegment(_selectedSegment - 1);
        OnChange?.Invoke();

    }
    /// <summary>
    /// Rotates the the orientation of the wire and indiviual bends clockwise.   
    /// </summary>
    public void RotateSegmentClockwise(float addedRotation)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        // rotate
        Segment currentSegment = _segmentList[_selectedSegment];
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Curve curve)
        {
            curve.AngleTwistDegrees = IncrementAngleDegrees(curve.AngleTwistDegrees, addedRotation);
        }
        ReplaceSegment(_selectedSegment, newSegmentData, addedRotation);
        OnChange?.Invoke();
    }
    
    /// <summary>
    /// Increases the bend of the selected curve segment.   
    /// </summary>
    public void ExtendCurvature(float curvatureChange)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        
        Segment currentSegment = _segmentList[_selectedSegment];
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Curve curve)
        {
            curve.CurvatureAngleDegrees = IncrementAngleDegrees(curve.CurvatureAngleDegrees, curvatureChange);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
            OnChange?.Invoke();
        }
    }
    
    /// <summary>
    /// Increases the curve distance from center.   
    /// </summary>
    public void ExtendCurveDistanceFromCenter(float distanceChange)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        
        Segment currentSegment = _segmentList[_selectedSegment];
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Curve curve)
        {
            curve.DistanceFromCenter = Mathf.Max(0.0f, curve.DistanceFromCenter + distanceChange);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
            OnChange?.Invoke();
        }
    }
    
    /// <summary>
    /// Increases the length of the selected straight line segment by 0.1 unity meters.   
    /// </summary>
    public void ExtendLine(float extension)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }

        // extend line
        Segment currentSegment = _segmentList[_selectedSegment];
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Line line)
        {
            line.Length = Mathf.Max(0.0f, line.Length + extension);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
            OnChange?.Invoke();
        }
    }
    /// <summary>
    /// Increases the length of the selected straight line segment by 0.1 unity meters.   
    /// </summary>
    public void SetLineLength(float length)
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }

        // extend line
        Segment currentSegment = _segmentList[_selectedSegment];
        Segment newSegmentData = currentSegment.Clone();
        if (newSegmentData is Line line)
        {
            line.Length = Mathf.Max(0.0f, length);
            ReplaceSegment(_selectedSegment, newSegmentData, 0.0f);
            OnChange?.Invoke();
        }
    }
    /// <summary>
    /// Selects the segment immediately ahead of the selected segment in the wire.   
    /// </summary>
    public void SelectNextSegment()
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        SetSelectedSegment(_selectedSegment + 1);
    }
    /// <summary>
    /// Selects the segment immediately behind the selected segment in the wire.   
    /// </summary>
    public void SelectPreviousSegment()
    {
        if (_segmentList.Count == 0 || _selectedSegment == -1)
        {
            return;
        }
        SetSelectedSegment(_selectedSegment - 1);
    }
    /// <summary>
    /// Prints raw Unity coordinates to Unity terminal in the following format: [x,y,z].  
    /// </summary>
    public void PrintCoordinatesAsArray()
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
    
    /// <summary>
    /// Scales the coordinates from "Unity meters" to centimeters and saves a csv file with the coordinates to produce the wire rendered on the Wire Terminal Software. 
    /// </summary>
    public void ExportCoordinates2CSV()
    {
        // CSV content builder
        string csvContent = "";

        // Header
        csvContent += "X,Y,Z\n";

        // Build the CSV content
        foreach (Vector3 point in _wireRenderer.Positions)
        {
            // Multiply each coordinate by "unityScaleFactor" to convert from Unity meters (effectively mm) to centimeters. TODO: Need to confirm this with the machine output.
            int unityScaleFactor = 10; // TODO: Need to test the appropriate scale factor to map from Unity meters to centimeters
            Vector3 scaledPoint = point * unityScaleFactor;
            // Reorient the [x, y, z] to [z, x, y] to match WireTerminal Orientation of machine TODO: Need to confirm this again in the lab. 
            csvContent += scaledPoint.z.ToString("F", CultureInfo.InvariantCulture) + "," + // Z is the direction straight out of the machine
                          scaledPoint.x.ToString("F", CultureInfo.InvariantCulture)+ "," +  // X is the left or right relative to the machine
                          scaledPoint.y.ToString("F", CultureInfo.InvariantCulture)  + "\n"; // Y is the direction up or down relative to the machine
        }
        
        // Use Application.dataPath to build a relative path
        // Note: Application.dataPath points to the "Assets" folder in Unity project // Relative to the "Assets" folder
        // Relative path from the Assets directory
        string relativePath = Path.Combine("Output", "Coordinates.csv");

        // Construct the full path using Application.dataPath
        string fullPath = Path.Combine(Application.dataPath, relativePath);

        // Ensure the directory exists
        string directoryPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        // Write to file
        File.WriteAllText(fullPath, csvContent);
        
        // Normalize the path to use forward slashes
        fullPath = fullPath.Replace("\\", "/");
        
        Debug.Log($"Coordinates exported to CSV at: {fullPath}");
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
        
        OnSelectionChange?.Invoke();
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
        InsertNewCurve(segmentInsertionIndex, pointInsertionIndex, twistDegrees, 90.0f, 1.5f);
        
        PropagateChange(twistDegrees, segmentInsertionIndex + 1, false);
        
        SetSelectedSegment(segmentInsertionIndex);
        
        OnChange?.Invoke();
    }

    public void AppendNewCurve(float twistDegrees, float curvatureAngleDegrees, float distanceFromCenter)
    {
        int segmentInsertionIndex = _segmentList.Count;
        int pointInsertionIndex = _wireRenderer.GetPositionsCount();
        
        InsertNewCurve(segmentInsertionIndex, pointInsertionIndex, twistDegrees, curvatureAngleDegrees, distanceFromCenter);
        
        PropagateChange(twistDegrees, segmentInsertionIndex + 1, false);
        
        OnChange?.Invoke();
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
        
        OnChange?.Invoke();
    }

    public void AppendNewLine(float length)
    {
        int segmentInsertionIndex = _segmentList.Count;
        int pointInsertionIndex = _wireRenderer.GetPositionsCount();
        
        const float twistDegrees = 0.0f;
        InsertNewLine(segmentInsertionIndex, pointInsertionIndex, length);
        
        PropagateChange(twistDegrees, segmentInsertionIndex + 1, false);
        
        OnChange?.Invoke();
    }
    
    
    protected override void ReplaceSegment(int oldSegmentIndex, Segment newSegmentData, float twistChange)
    {
        base.ReplaceSegment(oldSegmentIndex, newSegmentData, twistChange);
        
        // Update selection
        SetSelectedSegment(_selectedSegment);
    }
}
