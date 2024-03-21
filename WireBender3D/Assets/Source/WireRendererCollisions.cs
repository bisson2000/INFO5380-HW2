using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireRendererCollisions : MonoBehaviour
{
    private WireRenderer _wireRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
        _wireRenderer.OnMeshGenerated += CalculateCollisions;
    }

    // Update is called once per frame
    void Update()
    {
        //float dist = GetClosestDistance(_myList[0], _myList[1], _myList[2], _myList[3]);
    }

    private void OnDestroy()
    {
        _wireRenderer.OnMeshGenerated -= CalculateCollisions;
    }

    private void CalculateCollisions()
    {
        HashSet<int> affectedPositions = new HashSet<int>();
        IReadOnlyList<Vector3> positions = _wireRenderer.Positions;
        float radius = _wireRenderer.Radius;
        
        for (int i = 0; i < positions.Count - 1; i++)
        {
            bool checkForIntersection = false;
            Vector3 currentDirection = positions[i + 1] - positions[i];
            for (int j = i + 1; j < positions.Count - 1; j++)
            {
                Vector3 nextDirection = positions[j + 1] - positions[j];

                float angle = Vector3.Angle(currentDirection, nextDirection);
                if (angle >= 90.0f)
                {
                    checkForIntersection = true;
                }

                if (j <= i + 1 || !checkForIntersection)
                {
                    continue;
                }

                DistanceHelper dist = GetClosestDistance(positions[i], positions[i + 1], positions[j], positions[j + 1]);

                if (dist.Distance < radius * 2.0f)
                {
                    Debug.Log("Collision at " + i + " with dist = " + dist.Distance);
                    affectedPositions.Add(i);
                    affectedPositions.Add(j);
                }
            }
        }

        _wireRenderer.SetSubmesh(affectedPositions, 2);
        
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
