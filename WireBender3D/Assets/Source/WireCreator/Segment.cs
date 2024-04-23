using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Segment
{
    public int StartPointIndex;
    public int EndPointIndex;

    protected Segment(int startPointIndex, int endPointIndex)
    {
        StartPointIndex = startPointIndex;
        EndPointIndex = endPointIndex;
    }

    public abstract Segment Clone();
}

public class Curve : Segment
{
    public float CurvatureAngleDegrees;
    public float AngleTwistDegrees;
    public float DistanceFromCenter;
    public Curve(int startPointIndex, int endPointIndex, float angleTwistDegrees, float curvatureAngleDegrees, float distanceFromCenter) 
        : base(startPointIndex, endPointIndex)
    {
        AngleTwistDegrees = angleTwistDegrees;
        CurvatureAngleDegrees = curvatureAngleDegrees;
        DistanceFromCenter = distanceFromCenter;
    }


    public override Segment Clone()
    {
        return new Curve(StartPointIndex, EndPointIndex, AngleTwistDegrees, CurvatureAngleDegrees, DistanceFromCenter);
    }
}

public class Line : Segment
{
    public float Length;
    public Line(int startPointIndex, int endPointIndex, float length) 
        : base(startPointIndex, endPointIndex)
    {
        Length = length;
    }

    public override Segment Clone()
    {
        return new Line(StartPointIndex, EndPointIndex, Length);
    }
}

