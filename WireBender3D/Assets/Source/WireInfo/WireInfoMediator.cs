using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class WireInfoMediator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI segmentType;
    [SerializeField] private TextMeshProUGUI segmentLength;
    [SerializeField] private TextMeshProUGUI segmentRotation;
    [SerializeField] private TextMeshProUGUI segmentCurvature;
    
    [SerializeField] private LineRenderer lineLeft;
    [SerializeField] private LineRenderer lineRight;
    [SerializeField] private LineRenderer lineLeftVertical;
    [SerializeField] private LineRenderer lineRightVertical;

    [SerializeField] private RectTransform canvas;

    
    private Camera _mainCamera;

    public void Start()
    {
        _mainCamera = Camera.main;
        transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
    }

    public void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
    }

    public void SetLine(Vector3 start, Vector3 end, Vector3 up, float verticalOffset)
    {
        // Set position
        float additionalVerticalOffset = canvas.sizeDelta.y / 2.0f;
        Vector3 translationUp = up * (verticalOffset + additionalVerticalOffset);
        start += translationUp;
        end += translationUp;
        Vector3 middle = Vector3.Lerp(start, end, 0.5f);
        transform.position = middle;
        
        float length = Vector3.Distance(start, end);
        
        // Set info
        segmentType.text = "Type: Line";
        segmentLength.text = "Length: " + length.ToString("F", CultureInfo.InvariantCulture);
        segmentRotation.enabled = false;
        segmentCurvature.enabled = false;

        // Set lines
        lineLeft.positionCount = 2;
        lineRight.positionCount = 2;
        lineLeftVertical.positionCount = 2;
        lineRightVertical.positionCount = 2;
        
        Vector3 startToEnd = (end - start).normalized;
        float lineLength = Mathf.Max((length - canvas.sizeDelta.x) / 2.0f);
        lineLeft.SetPositions(new Vector3[]
        {
            start,
            (start + startToEnd * lineLength)
        });
        lineRight.SetPositions(new Vector3[]
        {
            end,
            (end - startToEnd * lineLength)
        });
        lineLeftVertical.SetPositions(new Vector3[]
        {
            (start - up * additionalVerticalOffset),
            (start + up * additionalVerticalOffset)
        });
        lineRightVertical.SetPositions(new Vector3[]
        {
            (end - up * additionalVerticalOffset),
            (end + up * additionalVerticalOffset)
        });
    }

    public void SetCurve(Vector3 start, Vector3 middle, Vector3 end, Vector3 middleUp, float radius, Curve curve)
    {
        float curveLength = curve.CurvatureAngleDegrees * Mathf.Deg2Rad * radius; // L = Theta * radius
        
        // Set info
        segmentType.text = "Type: Curve";
        segmentLength.text = "Curve length: " + curveLength.ToString("F", CultureInfo.InvariantCulture);
        segmentRotation.text = "rotation: " + curve.AngleTwistDegrees.ToString("F", CultureInfo.InvariantCulture);
        segmentCurvature.text = "curvature: " + curve.CurvatureAngleDegrees.ToString("F", CultureInfo.InvariantCulture);
        
        // Set position
        float additionalVerticalOffset = canvas.sizeDelta.y / 2.0f;
        float lineLength = Mathf.Max((Vector3.Distance(end, start) - canvas.sizeDelta.x) / 2.0f);
        middle += middleUp * (radius + additionalVerticalOffset);
        transform.position = middle;
        
        Vector3 startToEnd = (end - start).normalized;
        float projectedSize = Vector3.Project(middle - start, startToEnd).magnitude;
        start = middle - startToEnd * projectedSize;
        end = middle + startToEnd * projectedSize;
        
        // Set lines
        lineLeft.positionCount = 2;
        lineRight.positionCount = 2;
        lineLeftVertical.positionCount = 2;
        lineRightVertical.positionCount = 2;
        
        
        lineLeft.SetPositions(new Vector3[]
        {
            start,
            (start + startToEnd * lineLength)
        });
        lineRight.SetPositions(new Vector3[]
        {
            end,
            (end - startToEnd * lineLength)
        });
        lineLeftVertical.SetPositions(new Vector3[]
        {
            (start - middleUp * additionalVerticalOffset),
            (start + middleUp * additionalVerticalOffset)
        });
        lineRightVertical.SetPositions(new Vector3[]
        {
            (end - middleUp * additionalVerticalOffset),
            (end + middleUp * additionalVerticalOffset)
        });
    }
    
}
