using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DestroyNetworked : MonoBehaviourPun
{
    [SerializeField]
    private float destroyTimer = 5f;
    private void Start()
    {
        if(PhotonNetwork.IsMasterClient)
            Invoke("DestroyMe", destroyTimer);    
    }

    private void DestroyMe()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
