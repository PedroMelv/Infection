using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeakCell : Cell
{
    public override void PointerMove()
    {
        EventSystem.current.SetSelectedGameObject(transform.parent.gameObject);
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {

        CellArea.i.RemoveFromCells(this.gameObject);
    }
}
