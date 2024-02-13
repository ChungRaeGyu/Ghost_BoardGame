using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class LobbyManager : MonoBehaviour
{
    public Text NickName;
    public Text PlayerCount;

    // Start is called before the first frame update
    void Start()
    {
        NickName.text=PhotonNetwork.LocalPlayer.NickName+"님 환영합니다.";
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCount.text = "참가 플레이어 "+(PhotonNetwork.CountOfPlayers-PhotonNetwork.CountOfPlayersInRooms)+"명";
    }
}
