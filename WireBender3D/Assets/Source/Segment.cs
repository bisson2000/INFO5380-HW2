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

public class Curve : Segment
{
    public float PivotAngleDegrees;
    public float AngleDegrees;
    public Curve(int startPointIndex, int endPointIndex, float pivotAngleDegrees, float angleDegrees) : base(startPointIndex, endPointIndex)
    {
        PivotAngleDegrees = pivotAngleDegrees;
        AngleDegrees = angleDegrees;
    }
}

public enum SegmentType
{
    Line,
    Curve
}
