using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventAddForce : MonoBehaviour , IEventLanguageObjectListener , IControllerInputAcceptor
{

    public MeshRenderer myRenderer;
    public TextMesh myText;

    // basics
    private string[] myForceDirections = { "y", "x", "z" };
    private int myCurrentForceDirectionIndex = 0;

    // storage
    private List<DataReporter> myObjectsToAddForceTo;

    // chuck
    ChuckSubInstance myChuck;
    LanguageObject myLO;
    string myStorageClass;
    string myExitEvent;
    string myLastValue;

    // chuck value fetching
    Chuck.FloatCallback myValueFetchCallback;
    double myMostRecentValue;

    private void ChangeForceType()
    {
        myCurrentForceDirectionIndex++;
        myCurrentForceDirectionIndex %= myForceDirections.Length;
        SetForceText();
    }

    private void SetForceText()
    {
        myText.text = "add force:\n" + myForceDirections[myCurrentForceDirectionIndex];
    }

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myObjectsToAddForceTo = new List<DataReporter>();
        SetForceText();
        myLO = GetComponent<LanguageObject>();

        // init chuck
        // init chuck
        myChuck = chuck;
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();
        myLastValue = chuck.GetUniqueVariableName();
        myValueFetchCallback = Chuck.CreateGetFloatCallback( GetMyValue );

        chuck.RunCode(string.Format(@"
            global Event {1};
            global float {2};
            public class {0}
            {{
                static Gain @ myGain;
            }}
            Gain g @=> {0}.myGain;
            {0}.myGain => blackhole;

            fun void FetchValues()
            {{
                while( true )
                {{
                    {0}.myGain.last() => {2};
                    1::ms => now;
                }}
            }}

            spork ~ FetchValues();

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent, myLastValue ));
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        // cleanup chuck 
        myChuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    // callback fn
    private void GetMyValue( double newVal )
    {
        myMostRecentValue = newVal;
    }

    private void Update()
    {
        if( myChuck != null )
        {
            myChuck.GetFloat( myLastValue, myValueFetchCallback );
        }
    }

    public void TickDoAction()
    {
        // do nothing during Update() when receive an event
    }

    public void FixedTickDoAction()
    {
        // compute force to add
        Vector3 forceToAdd = Vector3.zero;
        switch( myForceDirections[myCurrentForceDirectionIndex] )
        {
            case "x":
                forceToAdd.x += (float) myMostRecentValue;
                break;
            case "y":
                forceToAdd.y += (float) myMostRecentValue;
                break;
            case "z":
                forceToAdd.z += (float) myMostRecentValue;
                break;
            default:
                break;
        }

        // add force to each of my linked objects
        foreach( DataReporter dr in myObjectsToAddForceTo )
        {
            dr.myRigidbody.AddForce( forceToAdd );
        }
    }

    public void NewListenEvent( ChuckSubInstance theChuck, string incomingEvent )
    {
        // don't care
    }

    public void LosingListenEvent( ChuckSubInstance theChuck, string losingEvent)
    {
        // don't care
    }

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care (will I ever have a parent?)
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care (will I ever have a parent?)
    }
    
    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        if( other is EventLanguageObject ||
            other.GetComponent<DataReporter>() != null || 
            other.GetComponent<NumberProducer>() != null ||
            other.GetComponent<SoundProducer>() != null )
        {
            return true;
        }
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // check for data reporter
        DataReporter maybeDataReporter = child.GetComponent<DataReporter>();
        if( maybeDataReporter != null )
        {
            // add to my objects to control
            myObjectsToAddForceTo.Add( maybeDataReporter );
        }

        // check for number producer or sound producer
        if( child.GetComponent<SoundProducer>() != null ||
            child.GetComponent<NumberProducer>() != null )
        {
            LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // check for data reporter
        DataReporter maybeDataReporter = child.GetComponent<DataReporter>();
        if( maybeDataReporter != null )
        {
            // remove from my objects to control
            myObjectsToAddForceTo.Remove( maybeDataReporter );
        }

        // check for number producer or sound producer
        if( child.GetComponent<SoundProducer>() != null ||
            child.GetComponent<NumberProducer>() != null )
        {
            LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
        }
    }

    public string VisibleName()
    {
        return "add force:\n" + myForceDirections[myCurrentForceDirectionIndex];
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        // same as output
        return OutputConnection();
    }

    public string OutputConnection()
    {
        // connection
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public void SizeChanged( float newSize )
    {
        // don't care
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // simulate touchpad presses until my index matches
        EventAddForce other = original.GetComponent<EventAddForce>();
        while( other.myCurrentForceDirectionIndex != myCurrentForceDirectionIndex )
        {
            TouchpadDown();
        }
    }


    public float[] SerializeFloatParams( int version )
    {
        // nothing to store
        return LanguageObject.noFloatParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // store my current index
        return new int[]{ myCurrentForceDirectionIndex };
    }

    public object[] SerializeObjectParams( int version )
    {
        // nothing to store
        return LanguageObject.noObjectParams;
    }

    public string[] SerializeStringParams( int version )
    {
        // nothing to store
        return LanguageObject.noStringParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams )
    {
        // simulate touchpad presses until my index matches
        while( intParams[0] != myCurrentForceDirectionIndex )
        {
            TouchpadDown();
        }
    }

    public void TouchpadDown()
    {
        ChangeForceType();
    }

    public void TouchpadUp()
    {
        // don't care
    }

    public void TouchpadAxis(Vector2 pos)
    {
        // don't care
    }

    public void TouchpadTransform(Transform touchpad)
    {
        // don't care
    }
}
