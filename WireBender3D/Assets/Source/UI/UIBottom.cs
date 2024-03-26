using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBottom : MonoBehaviour
{

    [SerializeField] private WireUserCreator _wireUserCreator;
    
    
    public void OnButtonPressed()
    {
        Debug.Log("Click");
    }

    public void OnValueChanged(string stringValue)
    {
        bool parsed = float.TryParse(stringValue, out float result);
        if (!parsed)
        {
            return;
        }
        Debug.Log(result);
        _wireUserCreator.SetLineLength(result);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
