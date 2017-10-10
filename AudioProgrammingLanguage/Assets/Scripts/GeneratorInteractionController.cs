using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorInteractionController : MonoBehaviour {

    public GameObject myPalette;
    public GameObject myTrash;
    public GameObject myPortalGenerator;
    private bool myPaletteEnabled = false;
    private Vector2 previousFingerPosition = Vector2.zero;

    private static List<GeneratorInteractionController> us = null;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    public static void TurnOffAllPalettes()
    {
        for( int i = 0; i < us.Count; i++ )
        {
            us[i].DisablePalette();
        }
    }

    private void Awake()
    {
        // get controller
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        
        // populate list of all objects with this type
        if( us == null )
        {
            us = new List<GeneratorInteractionController>();
        }
        us.Add( this );
    }

	// Use this for initialization
	void Start () {
		UpdateActive();
	}

    public void DisablePalette()
    {
        myPaletteEnabled = false;
        UpdateActive();
    }

    void UpdateActive()
    {
        // always enable portal
        myPortalGenerator.SetActive( myPaletteEnabled );

        // only enable palette and trash if renderers are rendering
        if( RendererController.renderersCurrentlyRendering )
        {
            myPalette.SetActive( myPaletteEnabled );
            myTrash.SetActive( myPaletteEnabled );
        }
        else
        {
            myPalette.SetActive( false );
            myTrash.SetActive( false );
        }
    }
	
	// Update is called once per frame
	void Update () {
        // set active
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.ApplicationMenu ) )
        {
            // ...
            myPaletteEnabled = !myPaletteEnabled;
            UpdateActive();
        }

        if( myPaletteEnabled )
        {
            // swipe to move the circular palette
            Vector2 currentFingerPosition = Controller.GetAxis();
            if( previousFingerPosition != Vector2.zero && currentFingerPosition != Vector2.zero )
            {
                // compute rotation amount
                float rotationAmount = Vector2.Angle( currentFingerPosition, previousFingerPosition );
                // going the opposite direcion?
                Vector3 crossProduct = Vector3.Cross( currentFingerPosition, previousFingerPosition );
                if( crossProduct.z < 0 )
                {
                    rotationAmount = 360 - rotationAmount;
                }
                // do the rotation!
                myPalette.transform.Rotate( new Vector3( 0, rotationAmount, 0) );
            }

            // store
            previousFingerPosition = currentFingerPosition;
        }
	}
}
