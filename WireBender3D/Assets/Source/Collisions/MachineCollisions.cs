using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MachineCollisions : WireCreator
{
    // Start is called before the first frame update
    [SerializeField] private WireCreator _referencedCreator;
    [SerializeField] private int _currentSegmentAnalysis = 1;

    private List<Segment> _sourceSegments = new List<Segment>();
    
    private bool _isReplaying = false;
    private MeshRenderer _meshRenderer;

    public override void Start()
    {
        base.Start();
        Collider a;
        //Physics.CheckCapsule();

        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isReplaying || true)
        {
            SaveSegments(_referencedCreator.SegmentList);
            SetSegments(_currentSegmentAnalysis);
            return;
        }
        
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && !_isReplaying)
        {
            // Save
            SaveSegments(_referencedCreator.SegmentList);
            
            // Set the new segments
            // SetSegments(_sourceSegments);

            _currentSegmentAnalysis = 1;
            _referencedCreator.gameObject.SetActive(false);
            _meshRenderer.enabled = true;
            _isReplaying = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && _isReplaying)
        {
            _referencedCreator.gameObject.SetActive(true);
            _meshRenderer.enabled = false;
            _isReplaying = false;
        }
    }

    private void SetSegments(int countFromLast)
    {
        // delete what was there
        for (int i = _segmentList.Count - 1; i >= 0; i--)
        {
            EraseSegment(i);
        }
        
        // Set all segments
        int startIndex = _sourceSegments.Count - countFromLast;
        Segment sourceSegment = _sourceSegments[startIndex];
        for (int i = 0; i < countFromLast; i++)
        {
            int indexInSource = startIndex + i;
            Segment newSegment = _sourceSegments[indexInSource].Clone();
            if (newSegment is Curve newCurve && sourceSegment is Curve sourceCurve)
            {
                newCurve.AngleTwistDegrees -= sourceCurve.AngleTwistDegrees;
            }
            
            newSegment.StartPointIndex -= sourceSegment.StartPointIndex;
            newSegment.EndPointIndex -= sourceSegment.StartPointIndex;
            InsertNewSegment(i, newSegment.StartPointIndex + 1, newSegment);
        }
    }

    private void SaveSegments(IReadOnlyList<Segment> segments)
    {
        _sourceSegments.Clear();
        foreach (Segment s in segments)
        {
            _sourceSegments.Add(s.Clone());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numSegment"> must be >0 </param>
    /*void PlaceSegmentFromEnd(int numSegment)
    {
        float removedTwist = _wireCreator.SegmentList[^numSegment].AngleTwistDegrees;
        
        List<Segment> newSegments = new List<Segment>();
        for (int i = numSegment; i > 0; i--)
        {
            Segment clone = _wireCreator.SegmentList[^i].Clone();
            clone.AngleTwistDegrees -= removedTwist;
            newSegments.Add(clone);
        }
    }*/
    
}
