using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    readonly string version = "1.0";
    string userID = "GPT";

    public TMP_InputField userIF;
    public TMP_InputField roomNameIF;

    Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    GameObject roomItemPrefab;
    public Transform scrollContent; // RoomItem 프리팹이 추가될 ScrollContent    

    RoomOptions ro = new RoomOptions();

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = version;
        Debug.Log(PhotonNetwork.SendRate);
        roomItemPrefab = Resources.Load<GameObject>("RoomItem");

        if (PhotonNetwork.IsConnected == false)
            PhotonNetwork.ConnectUsingSettings();
    }

    void Start()
    {
        userID = PlayerPrefs.GetString("USER_ID", $"ID_{Random.Range(1, 21):00}");
        userIF.text = userID;
        PhotonNetwork.NickName = userID;
    }

    //UserID 설정
    public void SetUserId()
    {
        if (string.IsNullOrEmpty(userIF.text))
            userID = $"ID_{Random.Range(1, 21):00}";
        else
            userID = userIF.text;

        PlayerPrefs.SetString("USER_ID", userID);
        PhotonNetwork.NickName = userID;
    }

    //Room Name 설정
    string SetRoomName()
    {
        if (string.IsNullOrEmpty(roomNameIF.text))
            roomNameIF.text = $"ROOM_{Random.Range(1, 101):000}";

        return roomNameIF.text;
    }

    //Master Server에 접속 후 호출되는 콜백 함수
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master!");
        Debug.Log($"InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }

    //로비에 접속 후 호출되는 콜백 함수
    public override void OnJoinedLobby()
    {
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        // PhotonNetwork.JoinRandomRoom();    //자동입장    
    }

    //Random Room 입장이 실패했을 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRandom Failed {returnCode}:{message}");
        OnMakeRoomClick();  //버튼클릭 후 실행될 함수
    }

    //Room 생성이 완료된 후 호출되는 콜백 함수
    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room");
        Debug.Log($"Room Name = {PhotonNetwork.CurrentRoom.Name}");
    }

    // 룸에 입장한 후 호출되는 콜백 함수
    public override void OnJoinedRoom()
    {
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
            Debug.Log($"{player.Value.NickName} , {player.Value.ActorNumber}");

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("BattleField");
    }

    // 룸 목록을 수신하는 콜백 함수
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 삭제된 RoomItem 프리팹을 저장할 임시변수
        GameObject tempRoom = null;

        foreach (var roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList == true)
            {
                rooms.TryGetValue(roomInfo.Name, out tempRoom); // 딕셔너리에서 룸 이름으로 검색해 저장된 RoomItem 프리팹를 추출
                Destroy(tempRoom);
                rooms.Remove(roomInfo.Name);    // 딕셔너리에서 해당 룸 이름의 데이터를 삭제
            }

            else // 룸 정보가 변경된 경우
            {
                if (rooms.ContainsKey(roomInfo.Name) == false) // 룸 이름이 딕셔너리에 없는 경우 새로 추가
                {
                    GameObject roomPrefab = Instantiate(roomItemPrefab, scrollContent); //RoomInfo 프리팹을 scrollContent 하위에 생성                    
                    roomPrefab.GetComponent<RoomData>().RoomInfo = roomInfo;    // 룸 정보를 표시하기 위해 RoomInfo 정보 전달

                    rooms.Add(roomInfo.Name, roomPrefab);   // 딕셔너리 자료형에 데이터 추가
                }

                else // 룸 이름이 딕셔너리에 없는 경우에 룸 정보를 갱신
                {
                    rooms.TryGetValue(roomInfo.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = roomInfo;
                }
            }

            Debug.Log($"Room={roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})");
        }
    }

    #region UI_BUTTON_EVENT
    public void OnLoginClick()
    {
        SetUserId();

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnMakeRoomClick()
    {
        SetUserId();
        ro.MaxPlayers = 20;
        ro.IsOpen = true;
        ro.IsVisible = true;

        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }
    #endregion
}
