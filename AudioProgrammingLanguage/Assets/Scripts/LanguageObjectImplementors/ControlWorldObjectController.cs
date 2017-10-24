using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
public class ControlWorldObjectController : MonoBehaviour , ILanguageObjectListener , IDataSource , IControllerInputAcceptor
{
    public MeshRenderer myShape;
    public TextMesh myText;

    private float myCurrent, myMin, myMax;
    private string myStorageClass, myExitEvent, mySampleGetter;


    private Chuck.FloatCallback myValueGetterCallback = null;

    private IControllable myParent = null;

    private int myControl = 0;
    private int myPrevControl = 0;
    private string[] defaultAllowedControls;
    private string[] myAllowedControls;


    private void Start () {
		myCurrent = 0;
        myMin = -1;
        myMax = 1;

        myValueGetterCallback = Chuck.CreateGetFloatCallback( GetMyCurrentValueCallback );

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
        TheChuck.Instance.GetFloat( mySampleGetter, myValueGetterCallback );
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
    }

    void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShape.material.color;
        myShape.material.color = temp;
    }

    // ILanguageObjectListener
    public bool AcceptableChild( LanguageObject other )
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

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public void NewChild( LanguageObject child )
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

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        ControlWorldObjectController other = original.GetComponent<ControlWorldObjectController>();
        // simulate touchpad presses until we are on the correct control
        while( myControl != other.myControl )
        {
            TouchpadDown();
        }
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // no string params
        return LanguageObject.noStringParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // store control index
        return new int[] { myControl };
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public object[] SerializeObjectParams( int version )
    {
        // no object params
        return LanguageObject.noObjectParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams )
    {
        // load control by simulating touchpad down
        while( myControl != intParams[0] )
        {
            TouchpadDown();
        }
    }
}
