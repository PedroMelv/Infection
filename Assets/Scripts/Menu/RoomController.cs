using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

[RequireComponent(typeof(PhotonView))]
public class RoomController : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameScene = "Game_Submarine";
    [Space]
    [SerializeField] private GameObject roomAreaObj;
    [SerializeField] private TextMeshProUGUI roomNameText;

    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startText;

    [SerializeField] private PlayerItem playerItemPrefab;
    [SerializeField] private Transform playersPlacement;
    private List<PlayerItem> playersList = new List<PlayerItem>();

    private Player[] playersOnline;
    private int cachedPlayerOneCharacter;
    private int cachedPlayerTwoCharacter;

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

        playersList[0].SetPlayerCharacter(cachedPlayerOneCharacter);
        if (playersList.Count >= 2)
        {
            playersList[1].SetPlayerCharacter(cachedPlayerTwoCharacter);
        }

        if (playersOnline.Length >= 2)
        {
            if (cachedPlayerOneCharacter != 0 && cachedPlayerTwoCharacter != 0 && cachedPlayerOneCharacter != cachedPlayerTwoCharacter)
            {
                startButton.interactable = true;
                startText.text = "Start Game";
            }
            else
            {
                startButton.interactable = false;
                startText.text = "Invalid";
            }
        }
        else
        {
            startButton.interactable = false;
            startText.text = "Invalid";
        }
    }

    public void EnterRoom()
    {
        ExitGames.Client.Photon.Hashtable playerConfig = PhotonNetwork.LocalPlayer.CustomProperties;

        if(playerConfig.ContainsKey("c"))
        {
            playerConfig["c"] = 0;
        }
        else
        {
            playerConfig.Add("c", 0);
        }
        

        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            cachedPlayerOneCharacter = (int)playerConfig["c"];
        }
        else
        {
            cachedPlayerTwoCharacter = (int)playerConfig["c"];
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerConfig);

        lobby.HideLobby();
        roomAreaObj.SetActive(true);
    }

    public void QuitRoom()
    {
        
        roomAreaObj.SetActive(false);
        PhotonNetwork.LeaveRoom();
        lobby.EnterLobby(false);
    }

    #region Callbacks
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

        for (int i = 0; i < playersOnline.Length; i++)
        {
            playersOnline[i].CustomProperties["c"] = 0;
        }

    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.IsMasterClient)
        {
            cachedPlayerOneCharacter = (int)changedProps["c"];
        }
        else
        {
            cachedPlayerTwoCharacter = (int)changedProps["c"];
        }
    }
    #endregion

    private void UpdateUI()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
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
        SceneManager.LoadScene(gameScene);
    }

    public void SetPlayerCharacter(int character)
    {
        ExitGames.Client.Photon.Hashtable customProp = PhotonNetwork.LocalPlayer.CustomProperties;
        if ((int)customProp["c"] == character) character = 0;
        customProp["c"] = character;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProp);
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
