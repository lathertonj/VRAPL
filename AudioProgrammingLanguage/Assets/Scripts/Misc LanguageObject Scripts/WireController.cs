using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireController : MonoBehaviour {

    // for changing size
    private MeshRenderer myRenderer;
    private float myXScale;
    private float myZScale;

    // for aligning endpoints
    private Transform myStart;
    private Transform myEnd;

    // for chuck connections
    private string myGain;
    private string myExitEvent;
    private ChuckSubInstance myChuck;
    private ILanguageObjectListener myStartLO;
    private ILanguageObjectListener myEndLO;
    private string myStartConnection = "";
    private string myEndConnection = "";


    private void Awake()
    {
        // renderer
        myRenderer = GetComponent<MeshRenderer>();
        myXScale = transform.localScale.x;
        myZScale = transform.localScale.z;
        UpdateTextureTiling();

        // moving
        myStart = transform;
        myEnd = transform;

        // my chuck gain
        myChuck = TheSubChuck.Instance;
        myGain = myChuck.GetUniqueVariableName();
        myExitEvent = myChuck.GetUniqueVariableName();
        myChuck.RunCode( string.Format( @"
            external Event {1};
            external Gain {0};
        ", myGain, myExitEvent ) );
    }

    private void Update()
    {
        try
        {
            // update to stay connected to myStart and myEnd
            SetEndpoints( myStart.position, myEnd.position );
        }
        catch( MissingReferenceException )
        {
            // one of my endpoints was destroyed. destroy myself.
            Destroy( gameObject );
        }
    }

    public void SetEndpoints( Transform start, Transform end )
    {
        // undo old chuck connection
        if( myStartConnection != "" && myEndConnection != "" )
        {
            myChuck.RunCode( string.Format( @"
                // disconnect input from myGain
                {0} =< external Gain {1};
                // disconnect myGain from output
                {1} =< {2};
                // assign null to myGain (will it crash?)
//                null @=> {1};
                ", 
                myStartConnection, myGain, myEndConnection
            ));
        }

        // store new connection
        myStart = start;
        myEnd = end;

        // maybe hook up to chuck
        myStartLO = (ILanguageObjectListener) start.GetComponent(typeof(ILanguageObjectListener));
        myEndLO = (ILanguageObjectListener) end.GetComponent(typeof(ILanguageObjectListener));
        LanguageObject startLanguageObject = start.GetComponent<LanguageObject>();

        // if they both have language objects, and the end object would accept the start one as a child,
        if( myStartLO != null && myEndLO != null &&
            myEndLO.AcceptableChild( startLanguageObject, myStartLO ) )
        {
            // then connect them to my gain!
            myStartConnection = myStartLO.OutputConnection();
            myEndConnection = myEndLO.InputConnection( startLanguageObject );
            myChuck.RunCode( string.Format(
                @"{0} => external Gain {1} => {2};", 
                myStartConnection, myGain, myEndConnection
            ));
        }
    }

    private void SetEndpoints( Vector3 start, Vector3 end )
    {
        // compute directional vector
        Vector3 offset = end - start;

        // set angle
        transform.up = offset;

        // set position
        transform.position = start + offset / 2;

        // set scale: 
        transform.localScale = new Vector3( myXScale, offset.magnitude / 2, myZScale );
        UpdateTextureTiling();
    }

    private void UpdateTextureTiling()
    {
        myRenderer.material.mainTextureScale = new Vector2( 1, transform.localScale.y * 50 );
    }
}
