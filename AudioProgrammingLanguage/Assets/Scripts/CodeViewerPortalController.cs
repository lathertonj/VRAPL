using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeViewerPortalController : MonoBehaviour {

    private bool shouldListenToCollisions = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    private void OnTriggerEnter( Collider other )
    {
        if( shouldListenToCollisions && other.gameObject == TheRoom.theEye.gameObject )
        {
            shouldListenToCollisions = false;

            if( RendererController.renderersCurrentlyRendering )
            {
                RendererController.TurnOff();
            }
            else
            {
                RendererController.TurnOn();
            }

            // apply fades. instantly to the portal color, then fade back to clear
            // purple color: DIVIDE RGB BY 2; make it .2 bluer as well
            SteamVR_Fade.Start( new Color( 0.33f / 2, 0, 0.75f / 2, 0.75f), 0f );
            SteamVR_Fade.Start( new Color( 0.33f / 2, 0, 0.75f / 2, 0 ), 1.5f );

            // delete self
            Destroy( gameObject );

        }
    }

    private void OnTriggerExit( Collider other )
    {
        
    }
}
