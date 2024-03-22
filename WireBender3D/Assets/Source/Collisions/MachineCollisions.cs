using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineCollisions : MonoBehaviour
{
    // Start is called before the first frame update
    /*
    [SerializeField] private WireCreator _wireCreator;

    private List<Segment> lastSavedSegments = new List<Segment>();

    [SerializeField] private int _nSegments = 1;

    private bool _isReplaying = false;
    
    void Start()
    {
        Collider a;
        Physics.CheckCapsule()
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !_isReplaying)
        {
            lastSavedSegments.Clear();
            foreach (Segment s in _wireCreator.SegmentList)
            {
                lastSavedSegments.Add(s.Clone());
            }
            
            // Set the new segments
            _wireCreator.SetSegments(new List<Segment>());
            
            _wireCreator.SetEnabledInputs(false);
            _isReplaying = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && _isReplaying)
        {
            _wireCreator.SetSegments(lastSavedSegments);
            
            _wireCreator.SetEnabledInputs(true);
            _isReplaying = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numSegment"> must be >0 </param>
    void PlaceSegmentFromEnd(int numSegment)
    {
        float removedTwist = _wireCreator.SegmentList[^numSegment].AngleTwistDegrees;
        
        List<Segment> newSegments = new List<Segment>();
        for (int i = numSegment; i > 0; i--)
        {
            Segment clone = _wireCreator.SegmentList[^i].Clone();
            clone.AngleTwistDegrees -= removedTwist;
            newSegments.Add(clone);
        }
    }
    */
}
