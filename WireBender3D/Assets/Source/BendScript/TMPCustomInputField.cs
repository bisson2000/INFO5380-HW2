using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPCustomInputField : TMP_InputField
{
    public Action<int> onCaretUpdateEvent;
    private int m_LastCaretPos = -1;
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        onCaretUpdateEvent?.Invoke(caretPosition);
    }
    
    public override void OnUpdateSelected(BaseEventData eventData)
    {

        base.OnUpdateSelected(eventData);

        if (!isFocused || m_LastCaretPos == caretPosition)
        {
            return;
        }
        
        m_LastCaretPos = caretPosition;
        onCaretUpdateEvent?.Invoke(caretPosition);
    }
}
