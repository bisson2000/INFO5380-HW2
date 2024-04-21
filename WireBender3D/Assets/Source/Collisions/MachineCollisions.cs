using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


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
    [SerializeField] 
    private InputActionProperty toogleMachineCollision = new InputActionProperty(new InputAction("Toggle Info", type: InputActionType.Button));

    [Tooltip("The referenced information component")]
    [SerializeField]
    private WireInfo _referencedInfo;
    
    // Start is called before the first frame update
    [SerializeField] private WireCreator _referencedCreator;
    private int _segmentAnalysisCount = 1;

    [SerializeField] private string layerName = "MachineMesh";
    private int _layerMask;

    [Min(0)] [SerializeField] private int _raycastsPerFrame = 1;
    private int _currentPointCounter = 0;

    [Tooltip("Units / frame")]
    [Min(1e-6f)]
    //[SerializeField] 
    private float _speed = 0.1f;
    private float _currentRatio = 0.0f;
    

    private List<Segment> _sourceSegments = new List<Segment>();
    private List<bool> _flipRotationAtPoint = new List<bool>();
    private List<float> _removedTwist = new List<float>();

    private HashSet<int> _foundCollisions = new HashSet<int>();

    private bool _isReplaying = false;
    private bool _isFinished = false;
    private MeshRenderer _meshRenderer;
    
    private void OnToggleMachineCollisions(InputAction.CallbackContext obj) // Mapped Key in Input Action (Key Mapped: "Backspace")
    {
        ToggleMachineCollisions();
    }
    

    public override void Start()
    {
        base.Start();
        _layerMask = 1 << LayerMask.NameToLayer(layerName);
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        
        toogleMachineCollision.action.performed += OnToggleMachineCollisions;
    }
    
    private void OnEnable()
    {
        toogleMachineCollision.action.Enable();
    }

    private void OnDisable()
    {
        toogleMachineCollision.action.Disable();
    }
    
    private void OnDestroy()
    {
        toogleMachineCollision.action.performed -= OnToggleMachineCollisions;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isReplaying && !_isFinished)
        {
            DetectCollisions();
        }
    }

    public void ToggleMachineCollisions()
    {
        if (!_isReplaying)
        {
            // remove previous results
            SetFoundCollisions(true);
            
            // Save
            SaveSegments(_referencedCreator.SegmentList);

            _isFinished = false;
            _segmentAnalysisCount = 1;
            if (_segmentAnalysisCount > _sourceSegments.Count)
            {
                _segmentAnalysisCount = _sourceSegments.Count;
                _isFinished = true;
            }

            _currentRatio = 1.0f;
            SetSegments(_segmentAnalysisCount);
            
            // Set visuals
            _referencedInfo.gameObject.SetActive(false);
            _referencedCreator.gameObject.SetActive(false);
            _meshRenderer.enabled = true;
            _isReplaying = true;
        }
        else
        {
            // reset visuals
            _referencedInfo.gameObject.SetActive(true);
            _referencedCreator.gameObject.SetActive(true);
            _meshRenderer.enabled = false;
            _isReplaying = false;
        }
        
        
    }

/// <summary>
    /// Save the segments for later use
    /// </summary>
    /// <param name="segments">The segments to save</param>
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
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            
            // propagate from previous
            _flipRotationAtPoint[i] = _flipRotationAtPoint[i + 1];
            _removedTwist[i] = _removedTwist[i + 1];
            
            if (segments[i] is Curve curve)
            {
                _removedTwist[i] = curve.AngleTwistDegrees;
                
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

    /// <summary>
    /// Set the current segments on the renderer
    /// </summary>
    /// <param name="countFromLast">The number of segments to take from the source, starting from the last</param>
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
        float twistAlteration = -1.0f * _removedTwist[startIndex] + (_flipRotationAtPoint[startIndex] ? 180.0f : 0.0f);
        
        
        for (int i = 0; i < countFromLast; i++)
        {
            int indexInSource = startIndex + i;
            Segment newSegment = _sourceSegments[indexInSource].Clone();
            newSegment.StartPointIndex = i == 0 ? 0 : _segmentList[i - 1].EndPointIndex;
            if (newSegment is Curve newCurve)
            {
                newCurve.AngleTwistDegrees = IncrementAngleDegrees(newCurve.AngleTwistDegrees, twistAlteration);
                if (i == 0)
                {
                    newCurve.CurvatureAngleDegrees *= _currentRatio;
                }
            }
            else if (i == 0 && newSegment is Line newLine)
            { 
                newLine.Length *= _currentRatio;
            }
            
            InsertNewSegment(i, newSegment.StartPointIndex + 1, newSegment);
        }
    }

    private void DetectCollisions()
    {
        int startIndex = _sourceSegments[^_segmentAnalysisCount].StartPointIndex;
        
        // Detect the collisions
        int segmentIndex = 0;
        for (int i = _currentPointCounter; i < _wireRenderer.Positions.Count - 1 && i < _currentPointCounter + _raycastsPerFrame; i++)
        {
            if (i >= SegmentList[segmentIndex].EndPointIndex)
            {
                segmentIndex++;
            }
            Vector3 start = transform.TransformPoint(_wireRenderer.Positions[i]);
            Vector3 end = transform.TransformPoint(_wireRenderer.Positions[i + 1]);
            bool hit = Physics.CheckCapsule(start, end, _wireRenderer.Radius, _layerMask);
            if (hit)
            {
                //int pointAbsoluteIndex = startIndex + i;
                int segmentAbsoluteIndex = _sourceSegments.Count - _segmentAnalysisCount + segmentIndex;
                _foundCollisions.Add(segmentAbsoluteIndex);
            }
        }

        _currentPointCounter = Mathf.Min(_currentPointCounter + _raycastsPerFrame, _wireRenderer.Positions.Count);

        // Everything was analysed for the current segment sublist
        if (_currentPointCounter >= _wireRenderer.Positions.Count)
        {
            _currentPointCounter = 0;
            if (_currentRatio < 1.0f)
            {
                _currentRatio += GetSegmentIncrement(_segmentAnalysisCount);
                _currentRatio = Mathf.Min(_currentRatio, 1.0f);
                SetSegments(_segmentAnalysisCount);
                return;
            }
            
            // Add another segment
            _segmentAnalysisCount++;

            if (_segmentAnalysisCount > _sourceSegments.Count)
            {
                _isFinished = true;
                SetFoundCollisions(false);
            }
            else
            {
                _currentRatio = Mathf.Min(GetSegmentIncrement(_segmentAnalysisCount), 1.0f);
                SetSegments(_segmentAnalysisCount);
            }
        }
    }

    /// <summary>
    /// Sets the feedback for detected collisions
    /// </summary>
    /// <param name="reset">Reset the feedback and the found collisions</param>
    private void SetFoundCollisions(bool reset)
    {
        HashSet<int> affectedPoints = new HashSet<int>();
        if (reset)
        {
            foreach (int i in _foundCollisions)
            {
                int count = _sourceSegments[i].EndPointIndex - _sourceSegments[i].StartPointIndex;
                affectedPoints.UnionWith(Enumerable.Range(_sourceSegments[i].StartPointIndex, count));
            }
            _wireRenderer.SetSubmesh(affectedPoints, 0);
            _foundCollisions.Clear();
        }
        
        affectedPoints.Clear();
        foreach (int i in _foundCollisions)
        {
            int count = _sourceSegments[i].EndPointIndex - _sourceSegments[i].StartPointIndex;
            affectedPoints.UnionWith(Enumerable.Range(_sourceSegments[i].StartPointIndex, count));
        }
        _wireRenderer.SetSubmesh(affectedPoints, 2);
        
    }

    private float GetSegmentIncrement(int countFromLast)
    {
        int startIndex = _sourceSegments.Count - countFromLast;
        Segment sourceSegment = _sourceSegments[startIndex];

        float increment = 0.0f;
        if (sourceSegment is Curve curve)
        {
            increment = _speed / (curve.CurvatureAngleDegrees * Mathf.Deg2Rad * curve.DistanceFromCenter);
        }
        else if (sourceSegment is Line line)
        {
            increment = _speed / line.Length;
        }

        return increment;
    }
    
}
