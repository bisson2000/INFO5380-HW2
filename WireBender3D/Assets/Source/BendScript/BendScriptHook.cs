using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BendScriptHook : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
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
        OnTextChanged?.Invoke(text);
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
        //TODO: throw new NotImplementedException();
    }
    
    /// <summary>
    /// Activate inputs
    /// </summary>
    /// <param name="arg0"></param>
    private void OnDeselect(string arg0)
    {
        _isWritingText = false;
        //TODO: throw new NotImplementedException();
    }
}
