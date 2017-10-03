using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorInteractionController : MonoBehaviour {

    public PaletteGeneratorController myPalette;
    private bool myPaletteEnabled = false;

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
		myPalette.gameObject.SetActive( myPaletteEnabled );
	}
	
	// Update is called once per frame
	void Update () {
		// send touchpad events
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.Touchpad ) )
        {
            //...
        }

        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.ApplicationMenu ) )
        {
            // ...
            myPaletteEnabled = !myPaletteEnabled;
            myPalette.gameObject.SetActive( myPaletteEnabled );
        }
	}
}
