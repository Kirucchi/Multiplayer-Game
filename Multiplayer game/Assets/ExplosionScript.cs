using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExplosionScript : MonoBehaviour {

    [HideInInspector]
    public string shooter;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetPhotonView().IsMine) {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(150f, shooter);
        }

    }

}
