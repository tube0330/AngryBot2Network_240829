using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject effect;
    public int actorNumber;

    void Start()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 1000.0f);
        Destroy(this.gameObject, 3.0f);
    }

    void OnCollisionEnter(Collision col)
    {
        var contact = col.GetContact(0);    // 충돌 지점의 첫 번째 정보 가져오기
        var effObj = Instantiate(effect, contact.point, Quaternion.LookRotation(-contact.normal));
        
        Destroy(effObj, 2.0f);
        Destroy(this.gameObject);
    }
}
