using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyBrain : MonoBehaviour
{

    protected EnemyMovement enemyMovement;
    public virtual void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public virtual void Start()
    {
        if(PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient == false)
        {
            Destroy(this);
        }
    }

    public virtual void Update()
    {

    }
}
