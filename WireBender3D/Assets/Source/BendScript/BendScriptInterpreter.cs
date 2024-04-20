using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class BendScriptInterpreter : MonoBehaviour
{
    [SerializeField]
    private BendScriptHook bendScriptHook;
    
    [SerializeField]
    private WireUserCreator wireUserCreator;
    
    // Start is called before the first frame update
    void Start()
    {
        bendScriptHook.OnTextChanged += OnTextChanged;
        wireUserCreator.OnChange += OnWireChange;
        wireUserCreator.OnSelectionChange += OnWireChange;
    }

    private void OnDestroy()
    {
        bendScriptHook.OnTextChanged -= OnTextChanged;
        wireUserCreator.OnChange -= OnWireChange;
        wireUserCreator.OnSelectionChange -= OnWireChange;
    }

    private void OnWireChange()
    {
        if (bendScriptHook.IsWritingText)
        {
            return;
        }
        
        CreateScriptFromBend();
    }

    private void OnTextChanged(string text, int caretPosition)
    {
        if (!bendScriptHook.IsWritingText)
        {
            return;
        }
        
        CreateBendFromText(text, caretPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateScriptFromBend()
    {
        IReadOnlyList<Segment> segmentList = wireUserCreator.SegmentList;
        StringBuilder stringBuilder =  new StringBuilder();

        int selectedLine = 0;
        int lineCounter = -1;
        float lastSeenRotation = 0.0f;
        for (int i = 0; i < segmentList.Count; i++)
        {
            // get local rotation
            if (segmentList[i] is Curve curve)
            {
                float localRotation = WireUserCreator.IncrementAngleDegrees(curve.AngleTwistDegrees, -1.0f * lastSeenRotation);
                stringBuilder.Insert(0, "rotate " + localRotation.ToString("F", CultureInfo.InvariantCulture) + "\n");
                
                stringBuilder.Insert(0, "\n");
                stringBuilder.Insert(0, ", " + curve.CurvatureAngleDegrees.ToString("F", CultureInfo.InvariantCulture));
                stringBuilder.Insert(0, "arc " + curve.DistanceFromCenter.ToString("F", CultureInfo.InvariantCulture));
                
                lastSeenRotation = curve.AngleTwistDegrees;
                lineCounter += 2;
            } 
            else if (segmentList[i] is Line line)
            {
                stringBuilder.Insert(0, "feed " + line.Length.ToString("F", CultureInfo.InvariantCulture) + "\n");
                lineCounter += 1;
            }

            if (i == wireUserCreator.SegmentList.Count - wireUserCreator.SelectedSegment - 1)
            {
                selectedLine = lineCounter;
            }
        }
        
        bendScriptHook.SetText(stringBuilder.ToString());
        
        // Set line colors
        bendScriptHook.lineColor.Clear();
        bendScriptHook.lineColor.Add(selectedLine, Color.yellow);
        bendScriptHook.DisplayLineColor();
    }

    private void CreateBendFromText(string text, int caretPosition)
    {
        string[] lines = text.Split("\n");
        List<Segment> segmentCreation = new List<Segment>();
        Dictionary<int, Color> syntaxErrorLines = new Dictionary<int, Color>();
        
        int targetIndex = -1;
        int caretPositionFromLast = text.Length - caretPosition;
        int selectedLine = 0;
        
        // TODO: Parse text

        float accumulatedRotation = 0.0f;
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string currentLine = lines[i].ToLower(CultureInfo.InvariantCulture);
            if (currentLine.StartsWith("feed "))
            {
                if (Single.TryParse(currentLine.Substring(5), NumberStyles.Float, CultureInfo.InvariantCulture, out float length))
                {
                    segmentCreation.Add(new Line(0, 0, length));
                }
                else
                {
                    syntaxErrorLines.Add(i, Color.red);
                }
                
                if (caretPositionFromLast >= 0)
                {
                    targetIndex++;
                }
            }
            else if (currentLine.StartsWith("arc "))
            {
                string[] args = currentLine.Substring(4).Split(',');
                if (args.Length == 2
                    && Single.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float radius)
                    && Single.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float angle))
                {
                    segmentCreation.Add(new Curve(0, 0, accumulatedRotation, angle, radius));
                    accumulatedRotation = 0.0f;
                }
                else
                {
                    syntaxErrorLines.Add(i, Color.red);
                }
                
                if (caretPositionFromLast >= 0)
                {
                    targetIndex++;
                }
            }
            else if (currentLine.StartsWith("rotate "))
            {
                if (Single.TryParse(currentLine.Substring(7), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
                {
                    accumulatedRotation = WireCreator.IncrementAngleDegrees(accumulatedRotation, parsedValue);
                }
                else
                {
                    syntaxErrorLines.Add(i, Color.red);
                }
            }
            else if (currentLine != "")
            {
                syntaxErrorLines.Add(i, Color.red);
            }

            if (caretPositionFromLast >= 0)
            {
                caretPositionFromLast -= currentLine.Length + 1;
                selectedLine = i;
            }
        }

        // Set line colors
        bendScriptHook.lineColor.Clear();
        if (syntaxErrorLines.Count > 0)
        {
            bendScriptHook.lineColor = syntaxErrorLines;
            bendScriptHook.DisplayLineColor();
            Debug.Log("Syntax error");
            return;
        }
        bendScriptHook.lineColor.Add(selectedLine, Color.yellow);
        bendScriptHook.DisplayLineColor();

        // Create segments
        wireUserCreator.EraseAllSegments();
        for (int i =  0; i < segmentCreation.Count; i++)
        {
            if (segmentCreation[i] is Curve curve)
            {
                wireUserCreator.AppendNewCurve(curve.AngleTwistDegrees, curve.CurvatureAngleDegrees, curve.DistanceFromCenter);
            }
            else if (segmentCreation[i] is Line line)
            {
                wireUserCreator.AppendNewLine(line.Length);
            }
        }
        wireUserCreator.SetSelectedSegment(targetIndex);
        
    }
}
