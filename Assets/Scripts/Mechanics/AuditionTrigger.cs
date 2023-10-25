using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AuditionTrigger : MonoBehaviourPun
{
    public SphereCollider col;
    public float triggerRange;
    public float destroyTimer = 5f;
    private bool destroying = false;

    private void Update()
    {
        col.radius = triggerRange;

        destroyTimer -= Time.deltaTime;

        if(PhotonNetwork.IsMasterClient && destroyTimer <= 0 && !destroying)
        {
            DestroyMe();
            destroying = true;
        }
    }

    private void DestroyMe()
    {
        photonView.RPC(nameof(RPC_Destroy), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Destroy()
    {
        Destroy(gameObject);
    }

    public static void InstantiateAuditionTrigger(Vector3 position, float range, float timer)
    {
        GameObject trigger = PhotonNetwork.Instantiate("AuditionTrigger", position, Quaternion.identity);
        AuditionTrigger aud = trigger.GetComponent<AuditionTrigger>();

        aud.triggerRange = range;
        aud.destroyTimer = timer;
    }
}
