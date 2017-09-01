using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCreator : MonoBehaviour {

    public Transform room;
    public GameObject userEye;
    public Transform portalPrefab;

    static private Transform myPortal;


    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

	// Use this for initialization
	void Start () {
	    myPortal = null;	
	}
	
	// Update is called once per frame
	void Update () {
		if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.ApplicationMenu ) )
        {
            if( myPortal == null && ! TheRoom.InAFunction() ) {
                // instantiate a new portal as a child of the room
                myPortal = Instantiate( portalPrefab, room );

                // position the portal adjacent to the viewer
                Vector3 viewerPosition = userEye.transform.position;
                viewerPosition.y = 0;
                myPortal.position = viewerPosition + new Vector3( 0.3f, 1.2f, 0.15f );
                myPortal.eulerAngles = new Vector3( 0, 26.57f, 0 );
            } 
            else
            {
                // destroy myportal
                Destroy( myPortal.gameObject );
                myPortal = null;
            }
        }
	}
}
