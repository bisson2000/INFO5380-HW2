using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Segment
{
    public int StartPointIndex;
    public int EndPointIndex;
    public float AngleTwistDegrees;

    protected Segment(int startPointIndex, int endPointIndex, float angleTwistDegrees)
    {
        StartPointIndex = startPointIndex;
        EndPointIndex = endPointIndex;
        AngleTwistDegrees = angleTwistDegrees;
    }

    public abstract Segment Clone();
}

public class Curve : Segment
{
    public float CurvatureAngleDegrees;
    public Curve(int startPointIndex, int endPointIndex, float angleTwistDegrees, float curvatureAngleDegrees) 
        : base(startPointIndex, endPointIndex, angleTwistDegrees)
    {
        CurvatureAngleDegrees = curvatureAngleDegrees;
    }


    public override Segment Clone()
    {
        return new Curve(StartPointIndex, EndPointIndex, AngleTwistDegrees, CurvatureAngleDegrees);
    }
}

public class Line : Segment
{
    public float Length;

    public Line(int startPointIndex, int endPointIndex, float angleTwistDegrees, float length) 
        : base(startPointIndex, endPointIndex, angleTwistDegrees)
    {
        Length = length;
    }

    public override Segment Clone()
    {
        return new Line(StartPointIndex, EndPointIndex, AngleTwistDegrees, Length);
    }
}

