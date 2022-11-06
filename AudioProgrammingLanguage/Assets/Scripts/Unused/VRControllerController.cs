using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class VRControllerController : MonoBehaviour {

    // 1
    private SteamVR_TrackedObject trackedObj;
    // 2
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

	// Use this for initialization
	void Awake () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Controller.GetAxis() != Vector2.zero)
        {
            //Debug.Log(gameObject.name + Controller.GetAxis());
        }


        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            
        }
	}
}
