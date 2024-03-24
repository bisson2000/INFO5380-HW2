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

    public void SetLine(float length)
    {
        segmentType.text = "Type: Line";
        segmentLength.text = "Length: " + length.ToString("F", CultureInfo.InvariantCulture);
        segmentRotation.enabled = false;
        segmentCurvature.enabled = false;
    }

    public void SetCurve(float radius, float rotation, float curvatureDegrees)
    {
        float length = curvatureDegrees * Mathf.Deg2Rad * radius; // L = Theta * radius
        
        segmentType.text = "Type: Curve";
        segmentLength.text = "Length: " + length.ToString("F", CultureInfo.InvariantCulture);
        segmentRotation.text = "rotation: " + rotation.ToString("F", CultureInfo.InvariantCulture);
        segmentCurvature.text = "curvature: " + curvatureDegrees.ToString("F", CultureInfo.InvariantCulture);
    }
    
}
