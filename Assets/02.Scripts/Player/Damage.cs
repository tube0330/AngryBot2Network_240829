using System.Collections;
using UnityEngine;
using Photon.Pun;
using Player = Photon.Realtime.Player;

public class Damage : MonoBehaviourPunCallbacks
{
    GameManager _gameManager;
    Transform tr;
    Animator ani;
    CharacterController cc;
    Renderer[] rens;   //사망 후 투명처리를 위한 MeshRenderer 선언

    int initHp = 100;
    public int Hp = 0;

    #region hash Animation
    readonly int hashDie = Animator.StringToHash("Die");
    readonly int hashRespawn = Animator.StringToHash("Respawn");
    #endregion

    void Awake()
    {
        tr = transform;
        rens = GetComponentsInChildren<Renderer>();
        ani = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        Hp = initHp;
    }

    void OnCollisionEnter(Collision col)
    {
        if (Hp > 0 && col.collider.CompareTag("BULLET"))
        {
            Hp -= 20;

            if (Hp <= 0)
            {
                if (photonView.IsMine)
                {
                    var actorNo = col.collider.GetComponent<Bullet>().actorNumber;  //총알의 actorNumber를 추출하여 누가 발사했는지를 알아냄
                    Player _player = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);  //Photon 네트워크의 룸에서 발사자 가져오기

                    string msg = $"\n<color=#00ff00>{photonView.Owner.NickName}</color> is killed by <color=#ff0000>{_player.NickName}</color>";
                    
                    photonView.RPC("KillMessage", RpcTarget.AllBufferedViaServer, msg);    //모든 클라이언트에게 메시지 전송
                }

                StartCoroutine(PlayerDie());
            }
        }
    }

    [PunRPC]
    void KillMessage(string msg) => _gameManager.msgList.text += msg;

    IEnumerator PlayerDie()
    {
        cc.enabled = false;
        ani.SetBool(hashRespawn, false);    //respawn 비활성화
        ani.SetTrigger(hashDie);

        yield return new WaitForSeconds(3.0f);

        ani.SetBool(hashRespawn, true);     //respawn 활성화

        SetPlayerVisible(false);    //캐릭터 투명하게 처리

        yield return new WaitForSeconds(1.5f);

        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);
        tr.position = points[idx].position;

        Hp = initHp;
        SetPlayerVisible(true); //캐릭터 보이게 처리
        cc.enabled = true;
    }

    void SetPlayerVisible(bool isVisible)
    {
        for (int i = 0; i < rens.Length; i++)
            rens[i].enabled = isVisible;
    }
}
