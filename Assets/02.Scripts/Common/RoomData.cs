using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    TMP_Text roomInfoText;
    PhotonManager photonManager;

    RoomInfo _roomInfo;
    public RoomInfo RoomInfo
    {
        get { return _roomInfo; }
        set
        {
            _roomInfo = value;
            roomInfoText.text = $"{_roomInfo.Name} ({_roomInfo.PlayerCount}/{_roomInfo.MaxPlayers})";

            // 버튼 클릭 이벤트에 함수 연결
            GetComponent<Button>().onClick.AddListener(() => OnEnterRoom(_roomInfo.Name));
        }
    }

    void Awake()
    {
        roomInfoText = GetComponentInChildren<TMP_Text>();
        photonManager = GameObject.Find("PhotonManager").GetComponent<PhotonManager>();
    }

    void OnEnterRoom(string roomName)
    {
        photonManager.SetUserId();  //UserID 설정
        PhotonNetwork.JoinRoom(roomName);   //Room 접속
    }
}