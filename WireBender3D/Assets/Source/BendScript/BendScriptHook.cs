using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using ColorUtility = UnityEngine.ColorUtility;

public class BendScriptHook : MonoBehaviour
{
    [SerializeField] 
    private TMPCustomInputField _inputField;

    [SerializeField] 
    private TMP_Text _lineNumber;

    public Dictionary<int, Color> lineColor = new Dictionary<int, Color>();

    [SerializeField]
    [Tooltip("The input action map to disable when editing")]
    private InputActionAsset _creatorActionMap;
    
    public bool IsWritingText => _isWritingText;
    private bool _isWritingText = false;
 
    // callBack to mesh generated
    public event Action<string, int> OnTextChanged;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputField.onValueChanged.AddListener(OnTextChange);
        _inputField.onSelect.AddListener(OnSelect);
        _inputField.onDeselect.AddListener(OnDeselect);
        _inputField.onCaretUpdateEvent += OnCaretUpdateEvent;
        _inputField.onValidateInput += CustomValidateInput;
    }
    private char CustomValidateInput(string text, int charIndex, char addedChar)
    {
        // Check if the added character is a Tab
        if (addedChar == '\t')
        {
            return '\0'; // Return null character if Tab is pressed, effectively ignoring it
        }
        return addedChar; // Return the character unmodified if not Tab
    }
    
    
    private void OnCaretUpdateEvent(int caretPosition)
    {
        //OnCaretChanged?.Invoke(caretPosition);
        OnTextChanged?.Invoke(_inputField.text, caretPosition);
        //throw new NotImplementedException();
    }

    private void OnTextChange(string text)
    {
        DisplayLineColor(text);
        OnTextChanged?.Invoke(text, _inputField.caretPosition);
    }

    public void DisplayLineColor()
    {
        DisplayLineColor(_inputField.text);
    }

    private void DisplayLineColor(string text)
    {
        int nLines = text.Split('\n').Length;
        _lineNumber.text = "";
        for (int i = 1; i <= nLines; i++)
        {
            if (lineColor.ContainsKey(i - 1))
            {
                _lineNumber.text += "<color=#" + ColorUtility.ToHtmlStringRGB(lineColor[i - 1]) + "><b>" + i + "</b></color>";
            }
            else
            {
                _lineNumber.text += i;
            }
            _lineNumber.text += "\n";
        }
    }

    public void SetText(string text)
    {
        _inputField.text = text;
    }
    
    /// <summary>
    /// Deactivate inputs
    /// </summary>
    /// <param name="arg0"></param>
    private void OnSelect(string arg0)
    {
        _isWritingText = true;
        _creatorActionMap.Disable();
    }
    
    /// <summary>
    /// Activate inputs
    /// </summary>
    /// <param name="arg0"></param>
    private void OnDeselect(string arg0)
    {
        _isWritingText = false;
        _creatorActionMap.Enable();
    }
}
