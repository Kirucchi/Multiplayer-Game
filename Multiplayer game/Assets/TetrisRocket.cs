using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisRocket : MonoBehaviour {

    string shooter = "";

    private void OnDestroy() {
        shooter = gameObject.GetComponent<BulletMove>().playerName;
        GameObject explosion = Instantiate(Resources.Load<GameObject>("Explosion"), transform.position, Quaternion.identity);
        explosion.GetComponent<ExplosionScript>().shooter = shooter;
        Destroy(explosion, 0.25f);
    }

    private void Update() {
        float currentSpeed = gameObject.GetComponent<BulletMove>().speed;
        float newspeed = currentSpeed + (15f * Time.deltaTime);
        gameObject.GetComponent<BulletMove>().speed = newspeed;
    }

}
