using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireController : MonoBehaviour , IControllerInputAcceptor {

    public Transform myCylinder;
    public Color textColor;

    // for changing size
    public MeshRenderer myRenderer;
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
        myXScale = myCylinder.localScale.x;
        myZScale = myCylinder.localScale.z;
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

    private void UndoCurrentConnection()
    {
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
    }

    private void OnDestroy()
    {
        UndoCurrentConnection();
    }

    public void SetEndpoints( Transform start, Transform end )
    {
        // undo old chuck connection
        UndoCurrentConnection();

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
        myCylinder.up = offset;

        // set position
        transform.position = start + offset / 2;

        // set scale: 
        myCylinder.localScale = new Vector3( myXScale, offset.magnitude / 2, myZScale );
        UpdateTextureTiling();
    }

    private void UpdateTextureTiling()
    {
        myRenderer.material.mainTextureScale = new Vector2( 1, myCylinder.localScale.y * 50 );
    }

    bool touchpadJustPressed = false;
    Vector3 touchpadInitialPosition;
    Vector3 touchpadCurrentPosition;
    float verticalThreshold = 0.1f;
    TextMesh touchpadText = null;

    public void TouchpadDown()
    {
        touchpadJustPressed = true;  
        Debug.Log("TOUCHPAD PRESSED~");
    }

    public void TouchpadUp()
    {
        if( touchpadText != null )
        {
            Destroy( touchpadText.gameObject );
            if( touchpadCurrentPosition.y - touchpadInitialPosition.y > verticalThreshold )
            {
                // delete wire
                Destroy( gameObject );
            }
            else if( touchpadCurrentPosition.y - touchpadInitialPosition.y < -verticalThreshold )
            {
                // switch wire direction
                SetEndpoints( myEnd, myStart );
            }
            else
            {
                // do nothing
            }
        }
    }

    public void TouchpadAxis( Vector2 pos )
    {
        // don't care
    }

    public void TouchpadTransform( Transform touchpad )
    {
        touchpadCurrentPosition = touchpad.position;
        // check if this is the first time we heard about it
        if( touchpadJustPressed )
        {
            touchpadJustPressed = false;
            touchpadInitialPosition = touchpad.position;
            touchpadText = Instantiate( 
                BasicTextHolder.basicTextPrefab, touchpadInitialPosition, 
                Quaternion.identity, transform
            ).GetComponent<TextMesh>();
            // move it up a bit
            touchpadText.transform.localPosition += Vector3.up * 0.1f;
            // color it green
            touchpadText.color = textColor;
            // face it toward my head
            touchpadText.transform.rotation = Quaternion.LookRotation( 
                touchpadText.transform.position - TheRoom.theEye.position );
        }

        if( touchpadCurrentPosition.y - touchpadInitialPosition.y > verticalThreshold )
        {
            touchpadText.text = "delete wire?";
        }
        else if( touchpadCurrentPosition.y - touchpadInitialPosition.y < -verticalThreshold )
        {
            touchpadText.text = "reverse wire direction?";
        }
        else
        {
            touchpadText.text = "delete wire\n\u2191\n\u2193\nreverse wire direction";
        }
    }
}
