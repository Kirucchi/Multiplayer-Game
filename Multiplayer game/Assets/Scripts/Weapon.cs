using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject {

    public string weaponName;
    public Sprite weaponSprite;
    public List<GameObject> bullets;

    public float radius;
    public float shootDelay;
    public float reloadTime;
    public int maxAmmo;
    public int maxClip;

}
