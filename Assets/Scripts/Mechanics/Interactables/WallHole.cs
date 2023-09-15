using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Unity.VisualScripting;

public class WallHole : Interactable
{
    [SerializeField] private Transform sideA;
    [SerializeField] private Transform sideB;

    private bool isUsing;

    public override void Start()
    {
        base.Start();
        OnInteractAction += (GameObject who) => WallHoleInteract(who);
    }
    public bool WallHoleInteract(GameObject whoInteracted)
    {
        if (isUsing) return false;

        Transform closestSide = GetClosestSide(whoInteracted.transform.position);

        Transform opositeSide = GetOpositeSide(closestSide);

        //photonView.RPC("RPC_CallWallHoldeInteraction", RpcTarget.All, whoInteracted.GetComponent<PhotonView>().ViewID, );
        StartCoroutine(PassTheHole(whoInteracted, closestSide.transform.position, opositeSide.transform.position));

        return true;
    }


    [PunRPC]
    public void RPC_SetWallHoleUsed(bool to)
    {
        isUsing = to;
    }

    private IEnumerator PassTheHole(GameObject pass, Vector3 pointA, Vector3 pointB)
    {
        MovementBase move = pass.GetComponent<MovementBase>();

        move.SetCollisions(true);

        float prepSpeed = 5f;

        float passSpeed = 2.5f;

        if (PhotonNetwork.InRoom) photonView.RPC("RPC_SetWallHoleUsed", RpcTarget.All, true); else isUsing = true;

        move.canMove = false;

        while(!IsCloseTo(pass.transform.position, pointA))
        {
            pass.transform.position = Vector3.MoveTowards(pass.transform.position, pointA, prepSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        pass.transform.position = pointA;

        while (!IsCloseTo(pass.transform.position, pointB))
        {
            pass.transform.position = Vector3.MoveTowards(pass.transform.position, pointB, passSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        pass.transform.position = pointB;

        move.SetCollisions(false);

        move.canMove = true;

        if (PhotonNetwork.InRoom) photonView.RPC("RPC_SetWallHoleUsed", RpcTarget.All, false); else isUsing = false;

    }


    #region Logic
    private bool IsCloseTo(Vector3 who, Vector3 closeTo, float distance = .025f)
    {
        return Vector3.Distance(who, closeTo) < distance;
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

    public Transform GetOpositeSide(Vector3 side)
    {
        return GetOpositeSide(GetClosestSide(side));
    }
    #endregion
}
