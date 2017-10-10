using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class BillboardController : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
		transform.eulerAngles = new Vector3( 0.0f, InputTracking.GetLocalRotation(VRNode.Head).eulerAngles.y, 0.0f );	
    }
}
