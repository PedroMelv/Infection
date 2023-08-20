using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHole : Interactable
{
    [SerializeField] private Transform sideA;
    [SerializeField] private Transform sideB;

    public override void Start()
    {
        base.Start();
        OnInteractAction += (GameObject who) => WallHoleInteract(who);
    }
    public void WallHoleInteract(GameObject whoInteracted)
    {
        Transform closestSide = GetClosestSide(whoInteracted.transform.position);

        Transform opositeSide = GetOpositeSide(closestSide);

        photonView.RPC("RPC_CallWallHoldeInteraction", RpcTarget.All, whoInteracted.name, closestSide.gameObject.name, opositeSide.gameObject.name);
    }

    [PunRPC]
    public void RPC_CallWallHoldeInteraction(string who, string from, string to)
    {
        //PhotonNetwork.GetPhotonView(PhotonNetwork.LocalPlayer.UserId);
        Debug.Log("The player " + who + " Interacted with this WallHode " + "Going from " + from + " to " + to);
    }

    public Transform GetClosestSide(Vector3 interactedPos)
    {
        Transform result = sideA;
        List<Transform> list = new List<Transform>
        {
            sideB,
            sideA
        };

        float distance = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            float dist = Vector3.Distance(interactedPos, list[i].position);
            Debug.Log(list[i].gameObject.name + " Distance: " + dist);
            if (distance > dist)
            {
                distance = dist;
                result = list[i];
            }
        }
        return result;
    }

    public Transform GetOpositeSide(Transform side)
    {
        if (side == sideA) return sideB;
        else return sideA;
    }
}
