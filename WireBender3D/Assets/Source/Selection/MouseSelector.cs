using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelector : MonoBehaviour
{
    [SerializeField] 
    private WireUserCreator wireUserCreator;
    private WireRenderer _wireUserCreatorRenderer;

    [SerializeField] 
    private string layerName = "MainMesh";
    private int _layerMask;
    
    // Start is called before the first frame update
    void Start()
    {
        _layerMask = 1 << LayerMask.NameToLayer(layerName);
        _wireUserCreatorRenderer = wireUserCreator.gameObject.GetComponent<WireRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hitInfo = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            bool hit = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, _layerMask);
            if (hit && !isOverUI)
            {
                SetSelection(hitInfo.point);
            }
        }
    }

    /// <summary>
    /// Sets the selection based on the hit point
    /// </summary>
    /// <param name="hit">The hit point, in world space</param>
    private void SetSelection(Vector3 hit)
    {
        if (wireUserCreator.SegmentList.Count == 0)
        {
            return;
        }
        
        int targetSegment = 0;
        int candidate = 0;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < _wireUserCreatorRenderer.Positions.Count - 1; i++)
        {
            if (i >= wireUserCreator.SegmentList[targetSegment].EndPointIndex)
            {
                targetSegment++;
            }

            Vector3 start = _wireUserCreatorRenderer.transform.TransformPoint(_wireUserCreatorRenderer.Positions[i]);
            Vector3 end = _wireUserCreatorRenderer.transform.TransformPoint(_wireUserCreatorRenderer.Positions[i + 1]);
            float dist = DistToSegment(start, end, hit);

            if (dist < minDistance)
            {
                minDistance = dist;
                candidate = targetSegment;
            }
        }
        
        wireUserCreator.SetSelectedSegment(candidate);
    }

    /// <summary>
    /// Shortest distance between a point and a segment
    /// </summary>
    /// <param name="start">start of the segment</param>
    /// <param name="end">end of the segment</param>
    /// <param name="point">point</param>
    /// <returns>shortest distance between the point and the segment</returns>
    private float DistToSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 startToEnd = end - start;
        Vector3 startToPoint = point - start;

        if (Vector3.Dot(startToPoint, startToEnd) <= 0)
            return startToPoint.magnitude;
        
        Vector3 endToPoint = point - end;
        if (Vector3.Dot(endToPoint, startToEnd) >= 0)
            return endToPoint.magnitude;

        return Vector3.Cross(startToEnd, startToPoint).magnitude / startToEnd.magnitude;
    }
}
