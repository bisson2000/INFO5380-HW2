using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class WireUserCreatorFeedback : MonoBehaviour
{
    [SerializeField] 
    private WireUserCreator _wireUserCreator;
    private WireRenderer _wireUserRenderer;
    
    private WireRenderer _localWireRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireUserRenderer = _wireUserCreator.gameObject.GetComponent<WireRenderer>();

        _localWireRenderer = GetComponent<WireRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        PlaceMeasurements();
    }

    void PlaceMeasurements()
    {
        _localWireRenderer.EraseRange(0, _localWireRenderer.Positions.Count);
        if (_wireUserCreator.SegmentList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _wireUserRenderer.Positions.Count; i++)
        {
            (Vector3 pos, Quaternion rot) = _wireUserRenderer.GetPositionRotation(i);

            pos += WireRenderer.GetUp(pos, rot) * (_wireUserRenderer.Radius * 2);
            
            _localWireRenderer.AddPositionRotation(pos, rot);
        }

        HashSet<int> coloredPositions = new HashSet<int>();
        int lastSegmentIndex = -1;
        for (int i = 0; i < _wireUserCreator.SegmentList.Count; i++)
        {
            Segment segment = _wireUserCreator.SegmentList[i];
            
            if (segment is Curve)
            {
                int start = segment.StartPointIndex;
                int count = segment.EndPointIndex - start;
                coloredPositions.AddRange(Enumerable.Range(start, count));
            }
        }
        _localWireRenderer.SetSubmesh(coloredPositions, 1);
        
        
        return;
        int startPoint = 0;
        for (int i = 0; i < _wireUserCreator.SegmentList.Count; i++)
        {
            Segment current = _wireUserCreator.SegmentList[i];
            
            if (current is Line line)
            {
                for (int j = current.StartPointIndex + 1; j <= current.EndPointIndex; j++)
                {
                    // Vector3 pos = _wireRenderer.Positions[j];
                    // List<Vector3> added = new List<Vector3>() {pos, pos};
                    // 
                    // positions.AddRange(added);
                }
            }
            else if (current is Curve curve)
            {
                for (int j = current.StartPointIndex + 1; j <= current.EndPointIndex; j++)
                {
                    // Vector3 pos = _wireRenderer.Positions[j];
                    // List<Vector3> added = new List<Vector3>() {pos, pos};
                    // if (j != current.EndPointIndex)
                    // {
                    //     added.Add(pos);
                    //     added.Add(pos);
                    // }
                    // 
                    // positions.AddRange(added);
                }
            }
            
        }

        //_lineRenderer.positionCount = positions.Count;
        //_lineRenderer.SetPositions(positions.ToArray());
    }
}
