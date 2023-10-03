using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter();
    }


    public virtual void PointerClick()
    {

    }

    public virtual void PointerEnter()
    {

    }
}
