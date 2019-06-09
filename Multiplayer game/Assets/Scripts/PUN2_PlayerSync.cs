using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PUN2_PlayerSync : MonoBehaviourPun, IPunObservable {

    //List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
    public MonoBehaviour[] localScripts;
    //List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
    public GameObject[] localObject;

    //Values that will be synced over network
    bool latestFlip;
    string reloadingText;
    float healthbarPosition;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidbody;
    public Text reloadText;
    public RectTransform healthbar;
    

    // Use this for initialization
    void Start () {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (photonView.IsMine) {
            //Player is local
        }
        else {
            //Player is Remote, deactivate the scripts and object that should only be enabled for the local player
            for (int i = 0; i < localScripts.Length; i++) {
                localScripts[i].enabled = false;
            }
            for (int i = 0; i < localObject.Length; i++) {
                localObject[i].SetActive(false);
            }
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            //We own this player: send the others our data
            stream.SendNext(spriteRenderer.flipX);
            stream.SendNext(reloadText.text);
            stream.SendNext(healthbar.localPosition.x);
        }
        else {
            //Network player, receive data
            latestFlip = (bool)stream.ReceiveNext();
            reloadingText = (string)stream.ReceiveNext();
            healthbarPosition = (float)stream.ReceiveNext();
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            //transform.position += new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, 0) * lag;
            //rigidbody.position += rigidbody.velocity * lag;
        }
    }

    void Update() {
        if (!photonView.IsMine) {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            spriteRenderer.flipX = latestFlip;
            reloadText.text = reloadingText;
            healthbar.localPosition = new Vector3(healthbarPosition, healthbar.localPosition.y, 0);
        }
    }
    

    
}
