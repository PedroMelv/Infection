using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBoxInteractable : ItemInteractable
{
    protected override void GrabItem(GameObject whoInteracted)
    {
        if ((int)whoInteracted.GetPhotonView().Controller.CustomProperties["c"] == 1)
        {
            whoInteracted.GetComponent<PlayerCombat>().GetAmmo();
            CallDestroy();
            return;
        }
        base.GrabItem(whoInteracted);
    }
}
