using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakCell : Cell
{
    public override void PointerEnter()
    {
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        CellArea.i.RemoveFromCells(this.gameObject);
    }
}
