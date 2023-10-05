using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [Header("Menu")]
    [SerializeField] private GameObject lobbyObj;
    [SerializeField] private GameObject attemptConnectObj;
    [SerializeField] private TMP_InputField nicknameField;
    [SerializeField] private TMP_InputField roomNameField;

    [SerializeField] private TextMeshProUGUI serverMessageText;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createButton;

    private bool attemptingConnect;

    private MenuController menu;

    [Header("Lobby Networking")]
    [SerializeField] private float roomSpacing;
    [SerializeField] private RoomItem roomObject;
    [SerializeField] private Transform roomPlacement;
    private List<RoomItem> roomItemList = new List<RoomItem>();

   private const string NICK_KEY = "NICKNAME_KEY";

    private void Awake()
    {
        menu = GetComponent<MenuController>();
    }

    private void Start()
    {
        nicknameField.text = PlayerPrefs.GetString(NICK_KEY, "Random_" + Random.Range(1,9999).ToString("0000"));
        PhotonNetwork.NickName = nicknameField.text;
    }

    private void Update()
    {
        attemptConnectObj.SetActive((attemptingConnect == false && PhotonNetwork.IsConnectedAndReady == false));

        createButton.interactable = (string.IsNullOrEmpty(roomNameField.text) == false && PhotonNetwork.InLobby);
        refreshButton.interactable = true;
    }

    public void EnterLobby(bool runLogic = true)
    {
        lobbyObj.SetActive(true);

        if(runLogic) StartCoroutine(EEnterLobby());
    }

    private IEnumerator EEnterLobby()
    {
        int connectAttempts = 0;
        int connectAttemptsMax = 10000;

        attemptingConnect = true;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.GameVersion = "0.1a";


        while (PhotonNetwork.InRoom == true && connectAttempts < connectAttemptsMax)
        {
            connectAttempts++;
            yield return null;
        }

        connectAttempts = 0;
        
        if(PhotonNetwork.IsConnectedAndReady == false)
        {
            PhotonNetwork.ConnectUsingSettings();
        }


        serverMessageText.text = "Connecting to Master...";

        while (PhotonNetwork.IsConnectedAndReady == false && connectAttempts < connectAttemptsMax)
        {
            connectAttempts++;
            yield return null;
        }

        if (PhotonNetwork.IsConnectedAndReady == false)
        {
            serverMessageText.text = "";
            attemptingConnect = false;
            PhotonNetwork.Disconnect();
            yield break;
        }

        connectAttempts = 0;

        serverMessageText.text = "Connecting to Lobby...";

        while (PhotonNetwork.InLobby == false && connectAttempts < connectAttemptsMax)
        {
            connectAttempts++;
            yield return null;
        }

        if (PhotonNetwork.InLobby == false)
        {
            serverMessageText.text = "";
            attemptingConnect = false;
            PhotonNetwork.Disconnect();
            yield break;
        }

        serverMessageText.text = "";

        attemptingConnect = false;
    }

    public void HideLobby()
    {
        lobbyObj.SetActive(false);
    }

    public void QuitLobby()
    {
        PhotonNetwork.Disconnect();

        lobbyObj.SetActive(false);

        menu.EnterMenu();
    }

    public void ChangeNickname(string newNickname)
    {
        if (string.IsNullOrEmpty(newNickname)) return;

        PlayerPrefs.SetString(NICK_KEY, newNickname);
        PhotonNetwork.NickName = newNickname;
    }

    public void CreateRoom()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;
            options.IsVisible = true;
            options.IsOpen = true;

            ExitGames.Client.Photon.Hashtable playerConfig = PhotonNetwork.LocalPlayer.CustomProperties;

            playerConfig.Add("c", 0);

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerConfig);

            PhotonNetwork.JoinOrCreateRoom(roomNameField.text, options, null);
        }
    }

    #region Network Functions

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectou na master");
        PhotonNetwork.JoinLobby();

        base.OnConnectedToMaster();
    }

    public override void OnLeftRoom()
    {
        RefreshButton();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Entrou no lobby");
        base.OnJoinedLobby();
    }

    public void RefreshButton()
    {
        StartCoroutine(ERefresh());
    }
    public IEnumerator ERefresh()
    {
        if (PhotonNetwork.InLobby) PhotonNetwork.LeaveLobby();
        while (PhotonNetwork.InLobby) yield return null;
        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
    }

    #region RoomListing

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomListUpdate(roomList);
    }

    private void RoomListUpdate(List<RoomInfo> roomList)
    {
        //Limpando lista de salas
        for (int i = 0; i < roomItemList.Count; i++)
        {
            Destroy(roomItemList[i].gameObject);
        }
        roomItemList.Clear();

        //Criando nova lista
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomItem room = Instantiate(roomObject, roomPlacement);
            room.SetRoomName(roomList[i].Name);
            roomItemList.Add(room);
        }
        RectTransform rt = roomPlacement.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, roomSpacing * roomList.Count);
    }

    #endregion

    #endregion
}
