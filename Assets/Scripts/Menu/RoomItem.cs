using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class RoomItem : MonoBehaviour
{
    private string roomName;
    [SerializeField] private TextMeshProUGUI roomText;

    public void SetRoomName(string roomName)
    {
        this.roomName = roomName;
        roomText.text = this.roomName;
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}
