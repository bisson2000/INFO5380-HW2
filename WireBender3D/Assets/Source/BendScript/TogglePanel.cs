using UnityEngine;
using UnityEngine.EventSystems;  
using UnityEngine.UI;       

public class TogglePanel : MonoBehaviour, IDragHandler
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
        if (Input.GetKeyDown(KeyCode.B))
        {
     
            if (EventSystem.current.IsPointerOverGameObject())
            {
                ToggleSize();
            }
        }
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

   
    public void OnDrag(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.D)){
        panel.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
        }
    }
}
