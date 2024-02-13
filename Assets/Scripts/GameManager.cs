using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int startpointCheck=0;
    public bool gamestart=false;
    public Button GameStartBtn;
    public GameObject GameStartTxt;
    int readycheck=0;
    public GameObject mainCamera;
    PhotonView pv;
    float x;
    float Z = -4.5f;
    public bool[] checkempty = new bool[37];
    public Button ready;
    public Text announcement;
    public GameObject turn;
    Text turnTxt;

    float time = 1;
    float timer=0;
    public bool Move;
    public bool myturn=true;
    public Button turnoverBtn;
    public GameObject[] RedImage = new GameObject[4];
    public GameObject[] BlueImage = new GameObject[4];
    int Red=0;
    int Blue=0;

    public GameObject EndingPanel;
    public Text GameResult;

    public Text NickNmaeTxt;
    // Start is called before the first frame update
    void Start() {
        pv = GetComponent<PhotonView>();
        AddGhost();
        turnTxt = turn.GetComponent<Text>();   
    }
    // Update is called once per frame
    void Update()
    {
        RoomRefresh();
        if (readycheck>=2){
            if(PhotonNetwork.IsMasterClient){
                GameStartBtn.gameObject.SetActive(true);
                pv.RPC("readycheckRPC",RpcTarget.AllBuffered);
            }
        }
        if(gamestart){
            if(time> timer){
                timer += Time.deltaTime;
            }else{
                pv.RPC("GameStartTxtOffRPC",RpcTarget.AllBuffered);
            }
            if (myturn)
                turnTxt.text = "내 차례";
            else
                turnTxt.text = "상대 차례";
            if(Red==4){
                GameOver("패배");
            }
            if(Blue==4){
                GameOver("승리");
            }
        }
    }
    public void GoalIn(){
        GameOver("승리");
    }
    public void GameOver(string a){
        pv.RPC("GameReset",RpcTarget.AllBuffered);
        GameResult.text = a;
        if(a=="승리")
            pv.RPC("GameOverRPC", RpcTarget.OthersBuffered, "패배");
        else
            pv.RPC("GameOverRPC", RpcTarget.OthersBuffered, "승리");
    }
    [PunRPC]
    void GameReset(){
        EndingPanel.SetActive(true);
        for(int i=Red;i>0;i--){
            RedImage[Red - 1].SetActive(false);
        }
        for (int j = Blue; j > 0; j--)
        {
            BlueImage[Blue - 1].SetActive(false);
        }
    }
    [PunRPC]
    void GameOverRPC(string a){
        GameResult.text = a;
    }
    [PunRPC]
    void GameStartTxtOffRPC(){
        GameStartTxt.SetActive(false);
    }
    [PunRPC]
    void readycheckRPC(){
        readycheck = 0;
    }
    public void ReadyBtn(){
        if(PhotonNetwork.IsMasterClient)
        {
            if(checkempty[2]&& checkempty[3]&& checkempty[4]&& checkempty[5]&& 
                checkempty[8]&& checkempty[9]&& checkempty[10]&& checkempty[11])

            {
                ready.GetComponentInChildren<Text>().text = "준비완료";
                ready.GetComponent<Button>().interactable = false;
                announcement.text = "준비완료";
                pv.RPC("readyRPC", RpcTarget.AllBuffered);
            }
            else{
                announcement.text = "시작 위치에 유령을 배치해 주세요";
            }
        }
        else 
        {
            if (checkempty[26] && checkempty[27] && checkempty[28] && checkempty[29] &&
                           checkempty[32] && checkempty[33] && checkempty[34] && checkempty[35])
            
            {
                ready.GetComponentInChildren<Text>().text = "준비완료";
                ready.GetComponent<Button>().interactable = false;
                announcement.text = "준비완료";
                pv.RPC("readyRPC", RpcTarget.AllBuffered);
            }
            else{
                announcement.text = "시작 위치에 유령을 배치해 주세요";
            }
        }
    }
    [PunRPC]
    void readyRPC(){
        readycheck ++;
    }
    public void StartBtn(){
        pv.RPC("StartBtnRPC",RpcTarget.AllBuffered);
        if((UnityEngine.Random.Range(1, 10) % 2)==0){
            turncheck(true);
            pv.RPC("turnCheckRPC",RpcTarget.Others,false);
        }else{
            turncheck(false);
            pv.RPC("turnCheckRPC", RpcTarget.Others, true);
        }
        GameStartBtn.interactable=false;
    }
    [PunRPC]
    void StartBtnRPC()
    {
        gamestart = true;
        GameStartTxt.GetComponent<Text>().text = "게임시작";
        GameStartTxt.SetActive(true);
        turn.SetActive(true);
        turnoverBtn.gameObject.SetActive(true);
    }
    void turncheck(bool a){
        myturn = a;
        turnoverBtn.GetComponent<Button>().interactable = a;
    }
    [PunRPC]
    void turnCheckRPC(bool a){
        myturn = a;
        turnoverBtn.GetComponent<Button>().interactable = a;
    }
    public void turnOverBtn()
    {
        myturn = !myturn;
        Move=false;
        announcement.text = "안내";
        turnoverBtn.GetComponent<Button>().interactable = myturn;
        pv.RPC("turnOverRPC", RpcTarget.OthersBuffered);
    }
    [PunRPC]
    void turnOverRPC()
    {
        myturn = !myturn;
        turnoverBtn.GetComponent<Button>().interactable = myturn;
    }

    void AddGhost()
    {
        print("유령 생성");
        if (PhotonNetwork.IsMasterClient)
        {
            print("마스터");
            x = -6;
            for (int i = 0; i < 8; i++)
            {
                if (i % 2 == 0)
                {
                    PhotonNetwork.Instantiate("Ghost_red", new Vector3(x, 0, Z + i), Quaternion.identity, 0);
                }
                else
                {
                    PhotonNetwork.Instantiate("Ghost_blue", new Vector3(x, 0, Z + i), Quaternion.identity, 0);
                }
            }
        }
        else
        {
            print("슬레이브");
            Vector3 camera = new Vector3(44.237f, 180, 0);
            mainCamera.transform.rotation = Quaternion.Euler(camera);
            mainCamera.transform.position = new Vector3(0, 7.45f, 7.5f);
            x = 6;
            for (int i = 0; i < 8; i++)
            {
                if (i % 2 == 0)
                {
                    PhotonNetwork.Instantiate("Ghost_red", new Vector3(x, 0, Z + i), Quaternion.Euler(0, 180, 0), 0);
                }
                else
                {
                    PhotonNetwork.Instantiate("Ghost_blue", new Vector3(x, 0, Z + i), Quaternion.Euler(0, 180, 0), 0);
                }
            }
        }
    }

    public void ColorCheck(string a){
        if(a=="Red"){
            Red++;
            RedImage[Red-1].SetActive(true);
        }else{
            Blue++;
            BlueImage[Blue-1].SetActive(true);
        }
    }

    public void Restart(){
        if(PhotonNetwork.IsMasterClient){
            GameStartBtn.gameObject.SetActive(false);
        }
        RestartBtn();
    }
    void RestartBtn(){
        EndingPanel.SetActive(false);
        timer=0;
        turn.gameObject.SetActive(false);
        startpointCheck = 0;
        gamestart = false;
        readycheck = 0;
        ready.interactable=true;
        announcement.text = "안내";
        Move = false;
        turnoverBtn.gameObject.SetActive(false);
        myturn=true;
        for (int i = 0; i < checkempty.Length; i++)
        {
            if(checkempty[i]&&i!=0){
                checkempty[i] = false;
                Destroy(GameObject.Find("Ghost" + i));
            }
        }
        AddGhost();
    }
    void RoomRefresh()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if(i%2==0){
                NickNmaeTxt.text = NickNmaeTxt.text.Replace("Master", PhotonNetwork.PlayerList[i].NickName);
            }else{
                NickNmaeTxt.text = NickNmaeTxt.text.Replace("Slave", PhotonNetwork.PlayerList[i].NickName);
            }
        }
    }
}
