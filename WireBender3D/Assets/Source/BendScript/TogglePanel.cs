using UnityEngine;
using UnityEngine.EventSystems;  
using UnityEngine.UI;       

public class TogglePanel : MonoBehaviour
{
    public RectTransform panel;      
    public Vector2 expandedSize;       
    public Vector2 compressedSize;    
    private bool isExpanded = true;    

    void Start()
    {
       
        panel.sizeDelta = expandedSize;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)){
        // if (EventSystem.current.IsPointerOverGameObject())
        // {
        ToggleSize();
        }
        // }
    }

    private void ToggleSize()
    {
        if (isExpanded)
        {
            panel.sizeDelta = compressedSize;
        }
        else
        {
            panel.sizeDelta = expandedSize;
        }
        isExpanded = !isExpanded;
    }
}
