using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinishingHandler : MonoBehaviourPun
{
    private bool deliveredBlood;
    private bool deliveredAcid;

    [SerializeField] private Interactable bloodInteractable;
    [SerializeField] private Interactable acidInteractable;


    public void DeliverBlood()
    {
        deliveredBlood = true;

        Destroy(bloodInteractable);

        if (deliveredAcid) GoBackToMenu();

    }

    public void DeliverAcid()
    {
        deliveredAcid = true;

        Destroy(acidInteractable);

        if (deliveredBlood) GoBackToMenu();
    }

    public void GoBackToMenu()
    {
        photonView.RPC(nameof(RPC_CloseRoom), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_CloseRoom()
    {
        PhotonNetwork.Disconnect();
    }
}
