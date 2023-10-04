using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        PointerMove();
    }

    public virtual void PointerClick()
    {

    }

    public virtual void PointerEnter()
    {

    }
    public virtual void PointerExit()
    {

    }
    public virtual void PointerMove()
    {

    }
}
