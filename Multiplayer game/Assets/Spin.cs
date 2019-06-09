using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.Rotate(Vector3.forward, 70 * Time.deltaTime);
	}
}
