using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class RoomController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject roomAreaObj;
    [SerializeField] private TextMeshProUGUI roomNameText;

    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startText;

    [SerializeField] private PlayerItem playerItemPrefab;
    [SerializeField] private Transform playersPlacement;
    private List<PlayerItem> playersList = new List<PlayerItem>();

    private Player[] playersOnline;

    private PhotonView view;

    private LobbyController lobby;

    private void Awake()
    {
        lobby = GetComponent<LobbyController>();
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom == false) return;

        if (Input.GetKeyDown(KeyCode.G)) StartGameButton();

        int readyCount = 0;

        for (int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].ready) readyCount++;
        }

        if (readyCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            startButton.interactable = true;
            startText.text = "Start Game";
        }
        else
        {
            startButton.interactable = false;
            startText.text = readyCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        }
    }

    public void EnterRoom()
    {
        lobby.HideLobby();
        roomAreaObj.SetActive(true);
    }

    public void QuitRoom()
    {
        roomAreaObj.SetActive(false);
        PhotonNetwork.LeaveRoom();
        lobby.EnterLobby(false);
    }

    public override void OnJoinedRoom()
    {
        EnterRoom();
        playersOnline = PhotonNetwork.PlayerList;
        UpdateUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playersOnline = PhotonNetwork.PlayerList;
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playersOnline = PhotonNetwork.PlayerList;
        UpdateUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        playersOnline = PhotonNetwork.PlayerList;
        UpdateUI();
    }

    private void UpdateUI()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
        readyButton.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }
    private void UpdatePlayerList()
    {
        //Limpando a lista
        for (int i = 0; i < playersList.Count; i++)
        {
            Destroy(playersList[i].gameObject);
        }
        playersList.Clear();


        //Mostrar dono da sala
        PlayerItem item = Instantiate(playerItemPrefab, playersPlacement);
        item.SetPlayer(PhotonNetwork.MasterClient);
        item.SetIsMaster(false, true);
        item.SetIsOwner(PhotonNetwork.IsMasterClient);
        item.ready = true;
        playersList.Add(item);

        //Mostrar jogadores

        for (int i = 0; i < playersOnline.Length; i++)
        {
            if (playersOnline[i] != PhotonNetwork.MasterClient)
            {
                PlayerItem pi = Instantiate(playerItemPrefab, playersPlacement);
                pi.SetPlayer(playersOnline[i]);
                pi.SetIsMaster(PhotonNetwork.IsMasterClient, false);
                pi.SetIsOwner((playersOnline[i] == PhotonNetwork.LocalPlayer));
                playersList.Add(pi);
            }
        }
    }

    public void DisconnectButton()
    {
        QuitRoom();
        
    }
    public void ReadyButton()
    {
        view.RPC("RPC_ReadyButton", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.NickName);
    }

    public void StartGameButton()
    {
        SceneManager.LoadScene("Game_Submarine");
    }

    public PlayerItem GetPlayerItem(string playername) 
    {
        for (int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].GetPlayer().NickName == playername)
            {
                return playersList[i];
            }
        }
        return null;
    }

    public void KickPlayer(Player kickMe)
    {
        view.RPC("RPC_KickPlayer", kickMe);
    }

    #region RPCs
    [PunRPC]
    public void RPC_ReadyButton(string playerName)
    {
        PlayerItem player = GetPlayerItem(playerName);

        if(player != null)
        {
            player.SwitchReady();
        }
    }

    
    [PunRPC]
    public void RPC_KickPlayer()
    {
        DisconnectButton();
    }

    #endregion
}
