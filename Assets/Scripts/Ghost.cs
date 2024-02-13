using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Ghost : MonoBehaviour
{
    Camera maincamera;
    Vector3 startposition;
    Vector3 Beforeposition;
    bool pickup =false;
    int layerMask;
    GameObject point;
    string gname;
    new string name;
    int Iname;
    int nowLocation;
    int afterLocation;
    Renderer lastpointRenderer;
    Color lastpointColor;
    Color basicColor= new Color(255 / 255f, 255 / 255f, 255 / 255f, 0 / 255f);
    Color twinkleColor=new Color(20 / 255f, 255 / 255f , 0 / 255f , 80 / 255f);
    public Text announcement;
    PhotonView pv;
    GameManager gamemanager;
    private AudioSource theAudio;
    //[SerializeField] private AudioClip clip;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "Ghost";
        maincamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        announcement = GameObject.Find("announcement").GetComponent<Text>();
        startposition = transform.position;
        layerMask = 1 << LayerMask.NameToLayer("Point");
        pv = GetComponent<PhotonView>();
        gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gname = gameObject.name;
        theAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pickup){
            Ray ray = maincamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            //RayCast

            if(Physics.Raycast(ray,out hit,10000f,layerMask)){

                //Point Color
                
                if(lastpointRenderer==null){
                    point = hit.collider.gameObject;
                    name = point.name;
                    Iname = Convert.ToInt32(name);
                    lastpointRenderer = point.GetComponent<Renderer>();
                    lastpointColor = lastpointRenderer.material.color;
                    hit.collider.GetComponent<Renderer>().material.color = twinkleColor;
                }
                if(name!=hit.collider.name){
                    lastpointRenderer.material.color = lastpointColor;
                    lastpointRenderer=null;
                }
            }
        }
    }

    private Vector3 m_Offset;
    private float m_ZCoord;

    void OnMouseDown()
    {
        if(gamemanager.myturn){
            if(!gamemanager.Move){
                if(pv.IsMine){
                    pickup = true;
                    m_ZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
                    m_Offset = gameObject.transform.position - GetMouseWorldPosition();
                    pv.RPC("MouseDownRPC",RpcTarget.AllBuffered, Iname);
                    nowLocation = Iname;
                }
            }else{
                announcement.text = "이미 유령을 움직였습니다. 차례를 넘겨주세요";
            }
        }
    }
    [PunRPC]
    void MouseDownRPC(int a)
    {
        gamemanager.checkempty[a] = false;
    }
    void OnMouseUp() {
        if (gamemanager.myturn)
        {
            if (!gamemanager.Move)
            {
                theAudio.Play();
                if (pv.IsMine){
                    gamemanager.checkempty[0] = true; //for Reset
                    lastpointRenderer.material.color = basicColor;
                    pickup = false;
                    afterLocation = Iname;
                    putdownGhost(gamemanager.gamestart);
                }
            }
            else
            {
                announcement.text = "이미 유령을 움직였습니다. 차례를 넘겨주세요";
            }
        }
    }
    void OnMouseDrag()
    {
        if (gamemanager.myturn)
        {
            if (!gamemanager.Move)
            {
                if (pv.IsMine){
                transform.position = GetMouseWorldPosition() + m_Offset;
                }
            }
            else
            {
                announcement.text = "이미 유령을 움직였습니다. 차례를 넘겨주세요";
            }
        }
    }

    Vector3 GetMouseWorldPosition()//100,50
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = m_ZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    void putdownGhost(bool IsStart){
        if(!gamemanager.checkempty[Iname]){
            if(IsStart)
            {
                if(Math.Abs(nowLocation-afterLocation)==1|| Math.Abs(nowLocation - afterLocation) == 6){
                    moveghost(IsStart);
                    GoalInCheck();
                }
                else{
                    announcement.text="상,하,좌,우 한칸씩 가능합니다.";
                    transform.position = Beforeposition;
                    Iname = nowLocation;
                    pv.RPC("putdownGhostRPC",RpcTarget.AllBuffered,nowLocation);
                }
                
            }
            else
            {
                if (point.CompareTag("startposition"))
                {
                    moveghost(IsStart);
                }
                else{
                    announcement.text = "시작 지점에 배치해 주세요";
                    transform.position = startposition;
                    afterLocation = nowLocation;
                    transform.name=gname;
                }
            }
        }
        else{
            if(GameObject.Find(gname+Iname).GetComponent<PhotonView>().IsMine){
                announcement.text = "빈곳에 놔주세요";
                if (IsStart){
                    transform.position = Beforeposition;
                    Iname = nowLocation;
                    pv.RPC("putdownGhostRPC", RpcTarget.AllBuffered, nowLocation);
                }else{
                    Iname = 0;
                    transform.position = startposition;
                }
                afterLocation = nowLocation;
            }
            else{
                if (IsStart){
                    if (Math.Abs(nowLocation - afterLocation) == 1 || Math.Abs(nowLocation - afterLocation) == 6)
                    {
                        if (GameObject.Find(gname+Iname).CompareTag("Red")){
                            gamemanager.ColorCheck("Red");
                        }else
                            gamemanager.ColorCheck("Blue");
                        pv.RPC("DestoryGhost",RpcTarget.AllBuffered,Iname);
                        moveghost(IsStart);
                        GoalInCheck();
                        print("잡음");
                    }
                    else{
                        announcement.text = "상,하,좌,우 한칸씩 가능합니다.";
                        transform.position = Beforeposition;
                        Iname = nowLocation;
                        pv.RPC("putdownGhostRPC", RpcTarget.AllBuffered, nowLocation);
                    }
                }
            }
        }
    }
    void GoalInCheck(){
        print("Iname : " + Iname);
        print("point : "+point.name);
        switch(Iname){
            case 31: if(PhotonNetwork.IsMasterClient && point.CompareTag("Goal") && transform.CompareTag("Blue")){
                    gamemanager.GoalIn();
                }
                break;
            case 36:
                if (PhotonNetwork.IsMasterClient && point.CompareTag("Goal") && transform.CompareTag("Blue"))
                {
                    gamemanager.GoalIn();
                }
                break;
            case 1 :
                if (!PhotonNetwork.IsMasterClient && point.CompareTag("Goal") && transform.CompareTag("Blue"))
                {
                    gamemanager.GoalIn();
                }
                break;
            case 6:
                if (!PhotonNetwork.IsMasterClient && point.CompareTag("Goal") && transform.CompareTag("Blue"))
                {
                    gamemanager.GoalIn();
                }
                break;
        }
    }
    [PunRPC]
    void DestoryGhost(int a){
        Destroy(GameObject.Find(gname + a));
    }
    void moveghost(bool a){
        if(a){
            gamemanager.Move=true;
        }
        pv.RPC("putdownGhostRPC",RpcTarget.AllBuffered,Iname);
        announcement.text = "안내";
        transform.position = point.transform.position;
        Beforeposition= point.transform.position;
    }
    [PunRPC]
    void putdownGhostRPC(int a){
        gamemanager.checkempty[a] = true;
        gameObject.name = gname + a;
    }
}
