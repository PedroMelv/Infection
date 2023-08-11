using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] private Sprite masterSpr, readySpr, notReadySpr;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerIconImg;
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

    public void SetIcon(Sprite spr)
    {
        playerIconImg.sprite = spr;
    }

    public void SetIsMaster(bool isMaster, bool forceShowMasterSprite = false)
    {
        if (isMaster || forceShowMasterSprite) SetIcon(masterSpr);
        kickButton.SetActive(isMaster);
        promoteButton.SetActive(isMaster);
    }

    public void SwitchReady()
    {
        ready = !ready;
        SetIcon((ready ? readySpr : notReadySpr));
    }

    public void SetIsOwner(bool isOwner)
    {
        if (isOwner) playerNameText.color = Color.yellow;
    }

    public void KickPlayer()
    {
        PhotonNetwork.CloseConnection(connectedPlayer);
    }

    public void PromotePlayer()
    {
        PhotonNetwork.SetMasterClient(connectedPlayer);
    }

}
