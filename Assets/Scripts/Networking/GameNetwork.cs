using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameNetwork : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerChris, playerPenny;
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

    private void Update()
    {
        CheckHealthy();
    }

    private void CheckHealthy()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (playerObjects == null || playerObjects.Count == 0) return;

        bool everyoneIsDead = true;
        for (int i = 0; i < playerObjects.Count; i++)
        {
            if (!playerObjects[i].GetComponent<BaseHealth>().isDead)
            {
                everyoneIsDead = false;
            }
        }

        if (everyoneIsDead) { SceneManager.LoadScene(1); }
    }

    [PunRPC]
    public void RPC_CreatePlayer(int index)
    {
        Player[] players = PhotonNetwork.PlayerList;

        GameObject playerPrefab = playerChris;
        if ((int)players[index].CustomProperties["c"] == 2)
        {
            playerPrefab = playerPenny;
        }

        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab.name, transform.position + spawnPos + new Vector3(0f, 0f, 1.5f) * index, Quaternion.identity);


        playerObj.GetComponent<PhotonView>().TransferOwnership(players[index]);

        PlayerInput pInput = playerObj.GetComponent<PlayerInput>();

        
        pInput.playerBody.GetChild(0).RecursiveSetLayer(LayerMask.NameToLayer("MyBody"));


        GameObject cameraHolder = Instantiate(cameraPrefab, transform.position, Quaternion.identity);
        cameraHolder.GetComponentInChildren<PlayerCamera>().SetOwner(pInput);

        playerObjects.Add(playerObj);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(spawnPos, .25f);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        SceneManager.LoadScene(0);
    }
}
