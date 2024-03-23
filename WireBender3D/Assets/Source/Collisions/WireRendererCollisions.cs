using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireRendererCollisions : MonoBehaviour
{
    private WireRenderer _wireRenderer;

    [Tooltip("The number of comparisons to execute every frame")]
    [Min(0)]
    [SerializeField] 
    private int comparisonsPerFrame = 40;

    private List<Vector3> _savedPositions = new List<Vector3>();
    private float _savedRadius = 0.0f;

    private HashSet<int> _collisions = new HashSet<int>();

    private int _currentPreviousIndex = 0;
    private int _currentNextIndex = 1;
    private bool _canAnalyse = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
        _wireRenderer.OnMeshGenerated += PrepareCollisionDetection;
    }

    // Update is called once per frame
    void Update()
    {
        //float dist = GetClosestDistance(_myList[0], _myList[1], _myList[2], _myList[3]);
        if (!_canAnalyse)
        {
            return;
        }
        
        CalculateCollisions();
    }

    private void OnDestroy()
    {
        _wireRenderer.OnMeshGenerated -= PrepareCollisionDetection;
    }

    private void PrepareCollisionDetection()
    {
        _savedPositions = new List<Vector3>(_wireRenderer.Positions);
        _savedRadius = _wireRenderer.Radius;
        
        _collisions.Clear();

        _currentPreviousIndex = 0;
        _currentNextIndex = 1;

        _canAnalyse = true;
    }

    private void CalculateCollisions()
    {
        int comnparisonsCounter = 0;
        
        IReadOnlyList<Vector3> positions = _savedPositions;
        for (; _currentPreviousIndex < positions.Count - 1; _currentPreviousIndex++)
        {
            bool checkForIntersection = false;
            Vector3 currentDirection = positions[_currentPreviousIndex + 1] - positions[_currentPreviousIndex];
            for (; _currentNextIndex < positions.Count - 1; _currentNextIndex++)
            {
                Vector3 nextDirection = positions[_currentNextIndex + 1] - positions[_currentNextIndex];

                float angle = Vector3.Angle(currentDirection, nextDirection);
                if (angle >= 90.0f)
                {
                    checkForIntersection = true;
                }

                if (_currentNextIndex <= _currentPreviousIndex + 1 || !checkForIntersection)
                {
                    continue;
                }

                Vector3 aStart = positions[_currentPreviousIndex];
                Vector3 aEnd = positions[_currentPreviousIndex + 1];
                Vector3 bStart = positions[_currentNextIndex];
                Vector3 bEnd = positions[_currentNextIndex + 1];
                DistanceHelper dist = GetClosestDistance(aStart, aEnd, bStart, bEnd);

                if (dist.Distance < _savedRadius * 2.0f)
                {
                    _collisions.Add(_currentPreviousIndex);
                    _collisions.Add(_currentNextIndex);
                }

                comnparisonsCounter++;
                if (comnparisonsCounter >= comparisonsPerFrame)
                {
                    return;
                }
            }

            _currentNextIndex = _currentPreviousIndex + 2;
        }

        _canAnalyse = false;
        _wireRenderer.SetSubmesh(_collisions, 2);
        
    }

    private DistanceHelper GetClosestDistance(Vector3 aStart, Vector3 aEnd, Vector3 bStart, Vector3 bEnd)
    {
        DistanceHelper res = new DistanceHelper();
        
        Vector3 a = aEnd - aStart;
        Vector3 b = bEnd - bStart;
        
        float aMag = a.magnitude;
        float bMag = b.magnitude;
        
        a.Normalize();
        b.Normalize();

        Vector3 cross = Vector3.Cross(a, b);
        float denominator = cross.magnitude * cross.magnitude;

        // Lines are parallel
        if (denominator == 0)
        {

            float bStartDot = Vector3.Dot(a, (bStart - aStart));
            float bEndDot = Vector3.Dot(a, (bEnd - aStart));

            // segment b is behind
            if (bStartDot <= 0 && bEndDot <= 0)
            {
                // bStart is closer
                if (Mathf.Abs(bStartDot) < Mathf.Abs(bEndDot))
                {
                    res.PointA = aStart;
                    res.PointB = bStart;
                    res.Distance = (aStart - bStart).magnitude;
                    return res;
                }
                
                res.PointA = aStart;
                res.PointB = bEnd;
                res.Distance = (aStart - bEnd).magnitude;
                return res;
            }
            
            // segment b is in front
            if (bStartDot >= aMag && bEndDot >= aMag)
            {
                // bStart is closer
                if (Mathf.Abs(bStartDot) < Mathf.Abs(bEndDot))
                {
                    res.PointA = aEnd;
                    res.PointB = bStart;
                    res.Distance = (aEnd - bStart).magnitude;
                    return res;
                }
                
                res.PointA = aEnd;
                res.PointB = bEnd;
                res.Distance = (aEnd - bEnd).magnitude;
                return res;
            }

            res.PointA = Vector3.zero;
            res.PointB = Vector3.zero;
            res.Distance = (((bStartDot * a) + aStart) - bStart).magnitude;
            return res;
        }

        Matrix4x4 proximityA = Matrix4x4.identity;
        proximityA.SetColumn(0, bStart - aStart);
        proximityA.SetColumn(1, b);
        proximityA.SetColumn(2, cross);
        float ratioA = proximityA.determinant / denominator;
        
        Matrix4x4 proximityB = Matrix4x4.identity;
        proximityB.SetColumn(0, bStart - aStart);
        proximityB.SetColumn(1, a);
        proximityB.SetColumn(2, cross);
        float ratioB = proximityB.determinant / denominator;
 
        Vector3 closestA = aStart + (a * ratioA);
        Vector3 closestB = bStart + (b * ratioB);
 
 
        // Clamp projections
        if (ratioA < 0)
            closestA = aStart;
        else if (ratioA > aMag)
            closestA = aEnd;
 
        if (ratioB < 0)
            closestB = bStart;
        else if (ratioB > bMag)
            closestB = bEnd;
        
        if (ratioA < 0 || ratioA > aMag)
        {
            float dot = Vector3.Dot(b, (closestA - bStart));
            dot = Mathf.Clamp(dot, 0, bMag);
            closestB = bStart + (b * dot);
        }
        if (ratioB < 0 || ratioB > bMag)
        {
            float dot = Vector3.Dot(a, (closestB - aStart));
            dot = Mathf.Clamp(dot, 0, aMag);
            closestA = aStart + (a * dot);
        }

        res.PointA = closestA;
        res.PointB = closestB;
        res.Distance = Vector3.Distance(closestA, closestB);
        return res;
    }
    
    private class DistanceHelper
    {
        public Vector3 PointA = Vector3.zero;
        public Vector3 PointB = Vector3.zero;
        public float Distance = 0.0f;
    }
}
