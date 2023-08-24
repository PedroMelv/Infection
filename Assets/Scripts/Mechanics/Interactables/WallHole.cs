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
    public void WallHoleInteract(GameObject whoInteracted)
    {
        if (isUsing) return;

        Transform closestSide = GetClosestSide(whoInteracted.transform.position);

        Transform opositeSide = GetOpositeSide(closestSide);

        //photonView.RPC("RPC_CallWallHoldeInteraction", RpcTarget.All, whoInteracted.GetComponent<PhotonView>().ViewID, );
        StartCoroutine(PassTheHole(whoInteracted, closestSide.transform.position, opositeSide.transform.position));
    }

    [PunRPC]
    public void RPC_SetWallHoleUsed(bool to)
    {
        isUsing = to;
    }

    private IEnumerator PassTheHole(GameObject pass, Vector3 pointA, Vector3 pointB)
    {
        PlayerMovement pMove = pass.GetComponent<PlayerMovement>();

        pMove.SetCollisions(true);

        float prepSpeed = 10f;

        float passSpeed = 2.5f;

        photonView.RPC("RPC_SetWallHoleUsed", RpcTarget.All, true);

        pMove.canMove = false;

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

        pMove.SetCollisions(false);

        pMove.canMove = true;

        photonView.RPC("RPC_SetWallHoleUsed", RpcTarget.All, false);

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
    #endregion
}
