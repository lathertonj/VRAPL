using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionPortalController : MonoBehaviour {

    public FunctionController myFunctionBlock;

    private int myDebounce;

    private void Start()
    {
        myDebounce = 0;
    }

    private void Update()
    {
        if( myDebounce >= 0 )
        {
            myDebounce--;
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        if( myDebounce <= 0 && 
            other.gameObject == TheRoom.theEye.gameObject && 
            RendererController.renderersCurrentlyRendering )
        {
            // signal head entered
            myFunctionBlock.HeadEnteredPortal();
            // turn screen to red
            SteamVR_Fade.Start( new Color( 0.55f, 0, 0, 0.75f ), 0 );
            // slowly turn screen back to clear
            SteamVR_Fade.Start( new Color( 0.55f, 0, 0, 0 ), 2 );
            // debounce for 2 frames
            myDebounce = 2;
        }
    }
}
