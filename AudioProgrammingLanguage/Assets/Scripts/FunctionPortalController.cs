using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionPortalController : MonoBehaviour {

    public FunctionController myFunctionBlock;

    private void OnTriggerEnter( Collider other )
    {
        if( other.gameObject == TheRoom.theEye.gameObject && RendererController.renderersCurrentlyRendering )
        {
            myFunctionBlock.HeadEnteredPortal();
            SteamVR_Fade.Start( new Color( 0.55f, 0, 0, 0.75f ), 0 );
            SteamVR_Fade.Start( new Color( 0.55f, 0, 0, 0 ), 2 );
        }
    }
}
