using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Segment
{
    public int StartPointIndex;
    public int EndPointIndex;

    public Segment(int startPointIndex, int endPointIndex)
    {
        StartPointIndex = startPointIndex;
        EndPointIndex = endPointIndex;
    }
}

public class Curve
{
    public int StartPointIndex;
    public int EndPointIndex;
    public CurveData CurveData;
    public Curve(int startPointIndex, int endPointIndex, CurveData curveData)
    {
        StartPointIndex = startPointIndex;
        EndPointIndex = endPointIndex;
        this.CurveData = curveData;
    }
    
    public Curve(int startPointIndex, int endPointIndex)
    {
        StartPointIndex = startPointIndex;
        EndPointIndex = endPointIndex;
        this.CurveData = new CurveData();
    }
}

public class CurveData
{
    public float PivotAngleDegrees;
    public float AngleDegrees;
    public float DistanceFromEnd;

    public CurveData(float pivotAngleDegrees, float angleDegrees, float distanceFromEnd)
    {
        PivotAngleDegrees = pivotAngleDegrees;
        AngleDegrees = angleDegrees;
        DistanceFromEnd = distanceFromEnd;
    }
    
    public CurveData()
    {
        PivotAngleDegrees = 0;
        AngleDegrees = 0;
        DistanceFromEnd = 0;
    }

    public CurveData Clone()
    {
        return new CurveData(PivotAngleDegrees, AngleDegrees, DistanceFromEnd);
    }

    public static CurveData GetDifference(CurveData from, CurveData to)
    {
        float pivotAngleDegrees = to.PivotAngleDegrees - from.PivotAngleDegrees;
        float angleDegrees = to.PivotAngleDegrees - from.PivotAngleDegrees;
        float distanceFromEnd = to.PivotAngleDegrees - from.PivotAngleDegrees;
        return new CurveData(pivotAngleDegrees, angleDegrees, distanceFromEnd);
    }
}

public enum SegmentType
{
    Line,
    Curve
}
