using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class BillboardController : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
		transform.eulerAngles = new Vector3( 0.0f, UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head).eulerAngles.y, 0.0f );	
    }
}
