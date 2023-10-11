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

    private Vector3 offset;

    private bool isUsing;

    protected override void Start()
    {
        base.Start();
        OnInteractAction += (GameObject who) => WallHoleInteract(who);
    }
    public bool WallHoleInteract(GameObject whoInteracted, Vector3 offset)
    {
        if (isUsing) return false;

        this.offset = offset;

        Transform closestSide = GetClosestSide(whoInteracted.transform.position);

        Transform opositeSide = GetOpositeSide(closestSide);

        //photonView.RPC("RPC_CallWallHoldeInteraction", RpcTarget.All, whoInteracted.GetComponent<PhotonView>().ViewID, );
        StartCoroutine(PassTheHole(whoInteracted, closestSide.transform.position, opositeSide.transform.position));

        return true;
    }

    public bool WallHoleInteract(GameObject whoInteracted)
    {
        return WallHoleInteract(whoInteracted, Vector3.zero);
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
        move.GetComponent<Rigidbody>().isKinematic = true;

        float prepSpeed = 7.5f;

        float passSpeed = 5f;

        if (PhotonNetwork.InRoom) photonView.RPC("RPC_SetWallHoleUsed", RpcTarget.All, true); else isUsing = true;

        move.canMove = false;

        while(!IsCloseTo(pass.transform.position, pointA + offset))
        {
            Debug.Log("Adjusting");

            pass.transform.position = Vector3.MoveTowards(pass.transform.position, pointA + offset, prepSpeed * Time.deltaTime);
            yield return null;
        }

        pass.transform.position = pointA + offset;

        while (!IsCloseTo(pass.transform.position, pointB + offset))
        {
            Debug.Log("Passing");
            pass.transform.position = Vector3.MoveTowards(pass.transform.position, pointB + offset, passSpeed * Time.deltaTime);
            yield return null;
        }

        pass.transform.position = pointB + offset;

        move.SetCollisions(false);
        move.GetComponent<Rigidbody>().isKinematic = false;

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
            //Debug.Log(list[i].gameObject.name + " Distance: " + dist);
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
