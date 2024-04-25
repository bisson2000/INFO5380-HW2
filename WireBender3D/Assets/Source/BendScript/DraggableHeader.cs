using UnityEngine;
using UnityEngine.EventSystems;  // Required for the drag functionality

public class DraggableHeader : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform dragTarget;  // The target panel to move

    public void OnDrag(PointerEventData eventData)
    {
        dragTarget.anchoredPosition += eventData.delta;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Bring the panel to the front when clicked
        dragTarget.SetAsLastSibling();
    }
}
