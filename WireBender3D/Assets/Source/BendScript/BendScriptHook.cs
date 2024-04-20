using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BendScriptHook : MonoBehaviour
{
    [SerializeField] 
    private TMP_InputField _inputField;

    [SerializeField] 
    private TMP_Text _lineNumber;

    public HashSet<int> lineColor = new HashSet<int>();

    [SerializeField]
    [Tooltip("The input action map to disable when editing")]
    private InputActionAsset _creatorActionMap;
    
    public bool IsWritingText => _isWritingText;
    private bool _isWritingText = false;
 
    // callBack to mesh generated
    public event Action<string> OnTextChanged;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputField.onValueChanged.AddListener(OnTextChange);
        _inputField.onSelect.AddListener(OnSelect);
        _inputField.onDeselect.AddListener(OnDeselect);
    }

    private void OnTextChange(string text)
    {
        DisplayLineColor(text);
        OnTextChanged?.Invoke(text);
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
            if (lineColor.Contains(i - 1))
            {
                _lineNumber.text += "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + "><b>" + i + "</b></color>";
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
