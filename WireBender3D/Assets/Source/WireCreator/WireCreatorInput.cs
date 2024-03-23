using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireCreatorInput : MonoBehaviour
{
    [SerializeField] 
    private InputActionProperty createCurveAction = new InputActionProperty(new InputAction("Create curve", type: InputActionType.Button));
    
    [SerializeField] 
    private InputActionProperty createLineAction = new InputActionProperty(new InputAction("Create straight segment", type: InputActionType.Button));

    
    // private WireCreator _wireCreator;

    private WireUserCreator _wireUserCreator;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireUserCreator = GetComponent<WireUserCreator>();
        createCurveAction.action.performed += OnCreateCurve; 
        createLineAction.action.performed += OnCreateLine; 
    }

    private void OnCreateLine(InputAction.CallbackContext obj)
    {
        _wireUserCreator.AddNewLine();
    }

    private void OnEnable()
    {
        createCurveAction.action.Enable();
        createLineAction.action.Enable();
    }

    private void OnDisable()
    {
        createCurveAction.action.Disable();
        createLineAction.action.Disable();
    }

    private void OnDestroy()
    {
        createCurveAction.action.performed -= OnCreateCurve; 
        createLineAction.action.performed -= OnCreateLine;  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCreateCurve(InputAction.CallbackContext obj)
    {
        _wireUserCreator.AddNewCurve();
    }
    
}
