using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletMove : MonoBehaviour {

    public float speed;
    public float damage;
    private Vector3 startPosition;
    private Vector3 direction;
    private float createTime;
    [HideInInspector]
    public string playerName;
    [HideInInspector]
    public int bulletNumber;
	
	// Update is called once per frame
	void FixedUpdate () {
        float timePassed = (float)(Time.time - createTime);//PhotonNetwork.Time
        transform.position = Vector3.Lerp(transform.position, startPosition + direction * speed * timePassed, 0.5f);
	}

    public void SetDirection(Vector3 bulletDirection) {
        direction = bulletDirection;
    }
    public void setStartPosition(Vector3 position) {
        startPosition = position;
    }
    public void setCreateTime(float time) {
        createTime = time;
    }
    public void setPlayer(string name) {
        playerName = name;
    }
    public void setBulletNumber(int number) {
        bulletNumber = number;
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Wall")) {
            //GameObject.Find("_RoomController").GetComponent<PUN2_RoomController>().projectileHIt(bulletNumber);
            Destroy(gameObject);
        }
        //use photon ismine instead of comparetag
        //else if (collision.gameObject.GetPhotonView().IsMine && collision.gameObject.GetComponent<PlayerController>().playerName != player.GetComponent<PlayerController>().playerName) {
        else if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetPhotonView().IsMine && collision.gameObject.GetComponent<PlayerController>().playerName != playerName) {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(damage, playerName);
            GameObject.Find("_RoomController").GetComponent<PUN2_RoomController>().projectileHIt(bulletNumber);
            Destroy(gameObject);
        }
        
    }
    
}
