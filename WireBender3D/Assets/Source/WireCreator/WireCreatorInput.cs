using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireCreatorInput : MonoBehaviour
{
    [SerializeField] 
    private InputActionProperty createCurveAction = new InputActionProperty(new InputAction("Create curve", type: InputActionType.Button));

    private WireCreator _wireCreator;
    
    // Start is called before the first frame update
    void Start()
    {
        _wireCreator = GetComponent<WireCreator>();
        createCurveAction.action.performed += OnCreateCurve; 
    }
    
    private void OnEnable()
    {
        createCurveAction.action.Enable();
    }

    private void OnDisable()
    {
        createCurveAction.action.Disable();
    }

    private void OnDestroy()
    {
        createCurveAction.action.performed -= OnCreateCurve; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCreateCurve(InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }
    
}
