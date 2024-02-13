using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using WebSocketSharp;
using Photon.Pun.UtilityScripts;
using UnityEngine.EventSystems;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Text StatusText;
    public InputField NickNameInput;
    public InputField RoomInputText;
    public GameObject RoomNameList_Txt;
    public GameObject RoomListContent;
    string RoomName;
    List<RoomInfo> myList = new List<RoomInfo>();
    // Start is called before the first frame update
    private void Awake() {
        Screen.SetResolution(1080,600, false);
        PhotonNetwork.SendRate = 60; //값을 보내는 빈도이다.
        PhotonNetwork.SerializationRate = 30; //동기화 빈도
    }

    // Update is called once per frame
    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }
    public void Connect(){
        PhotonNetwork.ConnectUsingSettings();   
    }

    public override void OnConnectedToMaster()
    {
        print("서버접속 완료");
        if(PhotonNetwork.LocalPlayer.NickName.IsNullOrEmpty()){
            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        }      
        JoinLobby();
        PhotonNetwork.LoadLevel("Lobby");
    }
    public void JoinLobby()=>PhotonNetwork.JoinLobby();
    
    public override void OnJoinedLobby(){   
        print("입장");
        myList.Clear();
    }
    public void CreateRoom()=> PhotonNetwork.CreateRoom(RoomInputText.text, new RoomOptions {MaxPlayers=2});

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        
        int roomCount = roomList.Count;
        for(int i=0; i<roomCount;i++){
            Destroy(GameObject.Find("RoomNameListBtn(Clone)"));
        }
        for (int i=0; i<roomCount;i++){
            if(!roomList[i].RemovedFromList){       //if RoomOut
                if (!myList.Contains(roomList[i])){
                    myList.Add(roomList[i]); //if myList and roomList not the same, Add roomList[i] to myList
                    print("방 추가 ");
                }
                else {
                    myList[myList.IndexOf(roomList[i])] = roomList[i]; //To maintain the same position when the same value occurs
                //myList.Indexof(roomList[i]) : Index return for that object
                    print("방 정렬 " );
                }
            }
            else if (myList.IndexOf(roomList[i]) != -1) {
                myList.RemoveAt(myList.IndexOf(roomList[i]));
                print("방삭제");
            }
        }
        myListRefresh();
    }
    void myListRefresh(){
        int myListCount = myList.Count;
        for (int j = 0; j < myListCount; j++)
        {
            GameObject RoomNameList = Instantiate(RoomNameList_Txt);
            RoomNameList.GetComponent<Button>().onClick.AddListener(()=>ButtonJoinRoom());
            Text RoomBtnName = RoomNameList.GetComponentInChildren<Text>();
            RoomBtnName.text = myList[j].Name;
            RoomNameList.transform.SetParent(RoomListContent.transform, false);
            print("실행 " + j);
        }
    }
    public void ButtonJoinRoom()
    {
        print("이름 : " + EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text);
        PhotonNetwork.JoinRoom(EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text);
    }
    public override void OnCreatedRoom()
    {    
        RoomName=RoomInputText.text;
        print("방생성 완료" + RoomName);
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("방만들기 실패");
    }
    public void JoinRandomRoom(){
        PhotonNetwork.JoinRandomRoom();
        PhotonNetwork.LoadLevel("Room");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("방 랜덤참가 실패");
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(RoomInputText.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Room");
        print("방 입장");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("message : " + message);
        print("방참가 실패");
    }
    public void RoomOut(){
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
        print("방 나옴");
    }
}
