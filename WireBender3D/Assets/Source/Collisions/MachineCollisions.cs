using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Computes the collisions with the machine.
/// The wire is formed in 4 steps:
/// 1. Extend.
/// 2. rotate on self.
/// 3. Curve.
/// 4. loop back to 1 until  there are no segments to extend.
/// </summary>
public class MachineCollisions : WireCreator
{
    // Start is called before the first frame update
    [SerializeField] private WireCreator _referencedCreator;
    [SerializeField] private int _segmentAnalysisCount = 1;
    
    [Min(0)]
    [SerializeField] 
    private int _raycastsPerFrame = 1;
    private int _currentPointCounter = 0;

    private List<Segment> _sourceSegments = new List<Segment>();
    private List<bool> _flipRotationAtPoint = new List<bool>();
    private List<float> _removedTwist = new List<float>();

    private HashSet<int> _foundCollisions = new HashSet<int>();
    
    private bool _isReplaying = false;
    private bool _isFinished = false;
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
        if (_isReplaying && !_isFinished)
        {
            DetectCollisions();
        }
        
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && !_isReplaying)
        {
            // remove previous results
            SetFoundCollisions(true);
            
            // Save
            SaveSegments(_referencedCreator.SegmentList);
            
            // Set the new segments
            // SetSegments(_sourceSegments);

            _isFinished = false;
            _segmentAnalysisCount = 1;
            if (_segmentAnalysisCount > _sourceSegments.Count)
            {
                _isFinished = true;
                Debug.Log("Finished!");
            }
            SetSegments(_segmentAnalysisCount);
            
            // Set visuals
            _referencedCreator.gameObject.SetActive(false);
            _meshRenderer.enabled = true;
            _isReplaying = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && _isReplaying)
        {
            // reset visuals
            _referencedCreator.gameObject.SetActive(true);
            _meshRenderer.enabled = false;
            _isReplaying = false;
        }
    }
    
    private void SaveSegments(IReadOnlyList<Segment> segments)
    {
        _sourceSegments = new List<Segment>(segments.Count);
        _removedTwist = new List<float>(segments.Count + 1);
        _flipRotationAtPoint = new List<bool>(segments.Count + 1);
        List<float> localRotationAtPoint = new List<float>(segments.Count);

        if (segments.Count == 0)
        {
            return;
        }

        
        float lastSeenRotation = 0.0f;
        for (int i = 0; i < segments.Count; i++)
        {
            _sourceSegments.Add(segments[i].Clone());
            _removedTwist.Add(0.0f);
            _flipRotationAtPoint.Add(false);
            
            // get local rotation
            localRotationAtPoint.Add(0.0f);
            if (segments[i] is Curve curve)
            {
                localRotationAtPoint[i] = IncrementAngleDegrees(curve.AngleTwistDegrees, -1.0f * lastSeenRotation);
                lastSeenRotation = curve.AngleTwistDegrees;
            }
        }
        
        _flipRotationAtPoint.Add(false);
        _removedTwist.Add(0.0f);
        
        // get where to flip the rotations
        float lastLocalRotation = 0.0f;
        float copiedTwist = _removedTwist[^1];
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            //if (!Mathf.Approximately(copiedTwist,_removedTwist[i]))
            //{
            //    copiedTwist = _removedTwist[i];
            //}
            //_removedTwist[i] = copiedTwist;
            
            // propagate from previous
            _flipRotationAtPoint[i] = _flipRotationAtPoint[i + 1];
            _removedTwist[i] = _removedTwist[i + 1];
            
            if (segments[i] is Curve curve)
            {
                _removedTwist[i] = curve.AngleTwistDegrees;
                //_removedTwist[i + 1] = curve.AngleTwistDegrees; // TODO remove this. it just is more beautiful
                
                // Flip when the last rotation was pointing "downwards"
                bool flip = (lastLocalRotation >= 180.0f && !_flipRotationAtPoint[i + 1])
                            || (lastLocalRotation <= 180.0f && _flipRotationAtPoint[i + 1]);
                flip = flip && !Mathf.Approximately(lastLocalRotation, 0.0f); // keep current if no change
                if (flip)
                {
                    _flipRotationAtPoint[i] = !_flipRotationAtPoint[i + 1];
                }

                lastLocalRotation = localRotationAtPoint[i];
            }
        }
        
        _flipRotationAtPoint.RemoveAt(_flipRotationAtPoint.Count - 1);
        _removedTwist.RemoveAt(_removedTwist.Count - 1);
    }

    private void SetSegments(int countFromLast)
    {
        // delete what was there
        for (int i = _segmentList.Count - 1; i >= 0; i--)
        {
            EraseSegment(i);
        }

        if (_sourceSegments.Count == 0)
        {
            return;
        }
        
        // Set all segments
        int startIndex = _sourceSegments.Count - countFromLast;
        Segment sourceSegment = _sourceSegments[startIndex];
        float twistAlteration = -1.0f * _removedTwist[startIndex] + (_flipRotationAtPoint[startIndex] ? 180.0f : 0.0f);
        //twistAlteration = -1.0f * _removedTwist[startIndex];
        
        for (int i = 0; i < countFromLast; i++)
        {
            int indexInSource = startIndex + i;
            Segment newSegment = _sourceSegments[indexInSource].Clone();
            newSegment.StartPointIndex -= sourceSegment.StartPointIndex;
            newSegment.EndPointIndex -= sourceSegment.StartPointIndex;
            if (newSegment is Curve newCurve)
            {
                newCurve.AngleTwistDegrees = IncrementAngleDegrees(newCurve.AngleTwistDegrees, twistAlteration);
            }
            
            InsertNewSegment(i, newSegment.StartPointIndex + 1, newSegment);
        }
    }

    private void DetectCollisions()
    {
        int startIndex = _sourceSegments[^_segmentAnalysisCount].StartPointIndex;
        
        // Detect the collisions
        for (int i = _currentPointCounter; i < _wireRenderer.Positions.Count - 1; i++)
        {
            Vector3 start = transform.TransformPoint(_wireRenderer.Positions[i]);
            Vector3 end = transform.TransformPoint(_wireRenderer.Positions[i + 1]);
            bool hit = Physics.CheckCapsule(start, end, _wireRenderer.Radius);
            if (hit)
            {
                int pointAbsoluteIndex = startIndex + i;
                _foundCollisions.Add(pointAbsoluteIndex);
                Debug.Log("Collision at " + i);
            }
        }

        _currentPointCounter = Mathf.Max(_currentPointCounter + _raycastsPerFrame, _wireRenderer.Positions.Count);

        // Everything was analysed for the current segment sublist
        if (_currentPointCounter >= _wireRenderer.Positions.Count)
        {
            // Add another segment
            _segmentAnalysisCount++;
            _currentPointCounter = 0;

            if (_segmentAnalysisCount > _sourceSegments.Count)
            {
                _isFinished = true;
                SetFoundCollisions(false);
            }
            else
            {
                SetSegments(_segmentAnalysisCount);
            }
        }
        

        return;


        //for (int i = _currentSegmentCounter; i < _segmentList.Count; ++)
        //{
        //    for (int j = 0 j < s.EndPointIndex; j++)
        //    {
        //        Vector3 start = transform.TransformPoint(_wireRenderer.Positions[j]);
        //        Vector3 end = transform.TransformPoint(_wireRenderer.Positions[j + 1]);
        //        
        //        bool hit = Physics.CheckCapsule(start, end, _wireRenderer.Radius);
        //        Debug.DrawLine(start, end);
        //        if (hit)
        //        {
        //            Debug.Log("hit at " + j);
        //        }
        //    }
        //}
    }

    private void SetFoundCollisions(bool reset)
    {
        if (reset)
        {
            _wireRenderer.SetSubmesh(_foundCollisions, 0);
            _foundCollisions.Clear();
        }
        
        _wireRenderer.SetSubmesh(_foundCollisions, 2);
        
    }
    
}
