using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WireRendererCollisions : MonoBehaviour
{
    private WireRenderer _wireRenderer;

    [SerializeField] private List<Vector3> _myList = new List<Vector3>() { Vector3.zero, Vector3.zero,Vector3.zero,Vector3.zero};
    
    
    // Start is called before the first frame update
    void Start()
    {
        _wireRenderer = GetComponent<WireRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float dist = GetClosestDistance(_myList[0], _myList[1], _myList[2], _myList[3]);
        Debug.Log("Dist = " + dist);
    }

    public void CalculateCollisions()
    {
        
    }

    private float GetClosestDistance(Vector3 aStart, Vector3 aEnd, Vector3 bStart, Vector3 bEnd)
    {
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
                    return (aStart - bStart).magnitude;
                }
                return (aStart - bEnd).magnitude;
            }
            
            // segment b is in front
            if (bStartDot >= aMag && bEndDot >= aMag)
            {
                // bStart is closer
                if (Mathf.Abs(bStartDot) < Mathf.Abs(bEndDot))
                {
                    return (aEnd - bStart).magnitude;
                }
                return (aEnd - bEnd).magnitude;
            }

            return 0.0f;
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

        return Vector3.Distance(closestA, closestB);
    }
}
