using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class PlayerItem : MonoBehaviourPun
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject kickButton;
    [SerializeField] private GameObject promoteButton;

    public bool ready;

    private Player connectedPlayer;

    public Player GetPlayer() { return connectedPlayer; }
    public void SetPlayer(Player player) 
    { 
        connectedPlayer = player;
        playerNameText.text = connectedPlayer.NickName;
    }

    public void SetIsMaster(bool isMaster, bool showMasterSprite = false)
    {
        kickButton.SetActive(isMaster);
        promoteButton.SetActive(isMaster);
    }

    public void SwitchReady()
    {
        ready = !ready;
    }

    public void SetPlayerCharacter(int character)
    {
        switch (character)
        {
            case 0:
                playerNameText.alignment = TextAlignmentOptions.Center;
            break;
            case 1:
                playerNameText.alignment = TextAlignmentOptions.Left;
            break;
            case 2:
                playerNameText.alignment = TextAlignmentOptions.Right;
            break;
        }
    }

    public void KickPlayer()
    {
        FindObjectOfType<RoomController>().KickPlayer(connectedPlayer);
    }

    public void PromotePlayer()
    {
        PhotonNetwork.SetMasterClient(connectedPlayer);
    }

}
