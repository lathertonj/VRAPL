using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
public class ControlWorldObjectController : MonoBehaviour , ILanguageObjectListener , IDataSource , IControllerInputAcceptor
{
    // TODO: don't just "control height" -- make an interface for the WorldObject sayings what things can be controlled by it.
    // or have it just be something present on worldobjects, editable with public variables

    public MeshRenderer myShape;
    public TextMesh myText;

    private float myCurrent, myMin, myMax;
    private string myStorageClass, myExitEvent, mySampleGetter;


    private Chuck.FloatCallback myValueGetterCallback = null;
    private List<Chuck.FloatCallback> deadCallbacks;
    private int framesSinceCallback = 0;

    private IControllable myParent = null;

    private int myControl = 0;
    private int myPrevControl = 0;
    private string[] defaultAllowedControls;
    private string[] myAllowedControls;


    private void Start () {
		myCurrent = 0;
        myMin = -1;
        myMax = 1;
        deadCallbacks = new List<Chuck.FloatCallback>();

        defaultAllowedControls = new string[] { "object" };
        myAllowedControls = defaultAllowedControls;

        myStorageClass = TheChuck.Instance.GetUniqueVariableName();
        myExitEvent = TheChuck.Instance.GetUniqueVariableName();
        mySampleGetter = TheChuck.Instance.GetUniqueVariableName();

        TheChuck.Instance.RunCode( string.Format(
            @"
            external Event {1};
            external float {2};

            public class {0}
            {{
                static Gain @ myGain;
            }}
            Gain g @=> {0}.myGain;
            {0}.myGain => blackhole;

            fun void FetchValue()
            {{
                while( true )
                {{
                    {0}.myGain.last() => {2};
                    1::ms => now;
                }}
            }}
            spork ~ FetchValue();

            // wait for exit event
            {1} => now;
            
            ", myStorageClass, myExitEvent, mySampleGetter
        ) );

        UpdateText();
	}
	
	private void Update () {
        // fetch myGain.last() into myCurrent
        if (myValueGetterCallback == null)
        {
            myValueGetterCallback = TheChuck.Instance.GetFloat(mySampleGetter, GetMyCurrentValueCallback);
            framesSinceCallback = 0;
        }
        else
        {
            framesSinceCallback++;
        }

        if( framesSinceCallback > 2 )
        {
            deadCallbacks.Add( myValueGetterCallback );
            myValueGetterCallback = null;
            Debug.Log("num dead callbacks: " + deadCallbacks.Count.ToString() );
        }
	}

    private void UpdateText()
    {
        myText.text = @"control
" + myAllowedControls[myControl];
    }

    private void OnDestroy()
    {
        TheChuck.Instance.BroadcastEvent( myExitEvent );
    }

    void GetMyCurrentValueCallback( double value )
    {
        myCurrent = (float) value;
        myValueGetterCallback = null;
    }

    void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShape.material.color;
        myShape.material.color = temp;
    }

    // ILanguageObjectListener
    public bool AcceptableChild( LanguageObject other, Collider collisionWith )
    {
        if( other.GetComponent<SoundProducer>() != null ||
            other.GetComponent<NumberProducer>() != null )
        {
            return true;
        }

        return false;
    }



    public void GotChuck( ChuckInstance chuck )
    {
        // don't care, shouldn't ever get a chuck
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        // don't care, shouldn't ever get a chuck
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public void NewChild( LanguageObject child, Collider collisionWith )
    {
        // don't care
    }

    public void ChildDisconnected( LanguageObject child )
    {
        // don't care 
    }

    public void NewParent( LanguageObject parent )
    {
        // don't care, shouldn't ever have a LanguageObjct as a parent
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        // don't care, shouldn't ever have a LanguageObject as a parent
    }

    public void NewParent( IControllable parent )
    {
        myParent = parent;
        SwitchColors();
        myAllowedControls = parent.AcceptableControls();
        myControl = myPrevControl;
        UpdateText();
        parent.StartControlling( myAllowedControls[myControl], this );
    }

    public void ParentDisconnected( IControllable parent )
    {
        myParent = null;
        SwitchColors();
        parent.StopControlling( myAllowedControls[myControl], this );
        myAllowedControls = defaultAllowedControls;
        myPrevControl = myControl;
        myControl = 0;
        UpdateText();
    }

    // IDataSource
    public float MaxValue()
    {
        return myMax;
    }

    public float MinValue()
    {
        return myMin;
    }

    public float CurrentValue()
    {
        return myCurrent;
    }

    public float NormValue()
    {
        return Mathf.Clamp01( ( myCurrent - myMin ) / ( myMax - myMin ) );
    }

    public void TouchpadDown()
    {
        if( myParent != null )
        {
            myParent.StopControlling( myAllowedControls[myControl], this );
        }

        myControl++;
        myControl %= myAllowedControls.Length;
        UpdateText();

        if( myParent != null )
        {
            myParent.StartControlling( myAllowedControls[myControl], this );
        }
    }

    public void TouchpadUp()
    {
        // don't care
    }

    public void TouchpadAxis(Vector2 pos)
    {
        // don't care
    }

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public string VisibleName()
    {
        return myText.text;
    }
}
