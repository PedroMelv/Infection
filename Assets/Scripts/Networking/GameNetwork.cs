using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class GameNetwork : MonoBehaviourPunCallbacks
{
    [SerializeField] private string playerPrefab;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private Vector3 spawnPos;

    private List<GameObject> playerObjects = new List<GameObject>();

    public void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;


        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            int index = i;
            photonView.RPC("RPC_CreatePlayer", players[index], index);
        }
    }

    [PunRPC]
    public void RPC_CreatePlayer(int index)
    {
        Player[] players = PhotonNetwork.PlayerList;

        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab, transform.position + spawnPos + new Vector3(0f, 0f, 1.5f) * index, Quaternion.identity);


        playerObj.GetComponent<PhotonView>().TransferOwnership(players[index]);

        
        GameObject cameraHolder = Instantiate(cameraPrefab, transform.position, Quaternion.identity);

        cameraHolder.GetComponentInChildren<PlayerCamera>().SetOwner(playerObj.GetComponent<PlayerInput>());
        
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(spawnPos, .25f);
    }
}
