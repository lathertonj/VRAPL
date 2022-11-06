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

    private ChuckSubInstance myChuck;
    private LanguageObject myLO;

    private int myControl = 0;
    private int myPrevControl = 0;
    private string[] defaultAllowedControls;
    private string[] myAllowedControls;


    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object 
		myCurrent = 0;
        myMin = -1;
        myMax = 1;

        myLO = GetComponent<LanguageObject>();

        myValueGetterCallback = Chuck.CreateGetFloatCallback( GetMyCurrentValueCallback );

        defaultAllowedControls = new string[] { "object" };
        myAllowedControls = defaultAllowedControls;

        // init chuck
        myStorageClass = TheSubChuck.instance.GetUniqueVariableName();
        myExitEvent = TheSubChuck.instance.GetUniqueVariableName();
        mySampleGetter = TheSubChuck.instance.GetUniqueVariableName();

        myChuck = chuck;
        chuck.RunCode( string.Format(
            @"
            global Event {1};
            global float {2};

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

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }
	
	private void Update () {
        // fetch myGain.last() into myCurrent
        TheSubChuck.instance.GetFloat( mySampleGetter, myValueGetterCallback );
	}

    private void UpdateText()
    {
        myText.text = @"control
" + myAllowedControls[myControl];
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

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care, shouldn't ever have a LanguageObjct as a parent
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care, shouldn't ever have a LanguageObject as a parent
    }

    public void ParentConnected( IControllable parent )
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

    // ILanguageObjectListener
    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        if( other.GetComponent<SoundProducer>() != null ||
            other.GetComponent<NumberProducer>() != null )
        {
            return true;
        }

        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // if SoundProducer or NumberProducer (currently all children...)
        LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // if SoundProducer or NumberProducer (currently all children...)
        LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
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
