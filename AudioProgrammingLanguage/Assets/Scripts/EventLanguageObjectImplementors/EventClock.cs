using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventClock : MonoBehaviour , IEventLanguageObjectEmitter {

    public Transform myClockHand;
    public MeshRenderer myClockHandMesh;

    private string myStorageClass;
    private string myTriggerEvent;
    private string myExitEvent;

    private void Awake()
    {
        myClockHandMesh.material.color = Color.black;
    }

    public void StartEmitTrigger() 
    {
        ChuckInstance theChuck = TheChuck.Instance;
        myStorageClass = theChuck.GetUniqueVariableName();
        myTriggerEvent = theChuck.GetUniqueVariableName();
        myExitEvent = theChuck.GetUniqueVariableName();

        theChuck.RunCode( string.Format( @"
            external Event {1};
            external Event {2};

            public class {0}
            {{
                static Gain @ myGain;
                static Step @ myDefaultValue;
            }}

            Gain g @=> {0}.myGain;
            Step s @=> {0}.myDefaultValue;
            1 => {0}.myDefaultValue.next;
            {0}.myDefaultValue => {0}.myGain => blackhole;

            fun void BroadcastEvents()
            {{
                while( true )
                {{
                    {0}.myGain.last() => float secTimeToWait;
                    Math.max( secTimeToWait, 0.0001 ) => secTimeToWait;
                    secTimeToWait::second => now;
                    {2}.broadcast();
                }}
            }}

            // broadcast
            spork ~ BroadcastEvents();

            // wait until told to exit
            {1} => now;

            ", myStorageClass, myExitEvent, myTriggerEvent    
        ));
    }

    public string ExternalEventSource()
    {
        return myTriggerEvent;
    }

    public void ShowEmit()
    {
        // tick the clock hand forward by 5 degrees
        Vector3 rotation = myClockHand.localEulerAngles;
        rotation.z -= 5;
        myClockHand.localEulerAngles = rotation;
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
    }
    
    public bool AcceptableChild( LanguageObject other )
    {
        if( other.GetComponent<NumberProducer>() != null ||
            other is EventLanguageObject )
        {
            return true;
        }
        return false;
    }

    public void NewParent( LanguageObject parent )
    {
        // don't care (will I ever have a parent?)
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        // don't care (will I ever have a parent?)
    }

    public void NewChild( LanguageObject child )
    {
        // don't care
    }

    public void ChildDisconnected( LanguageObject child )
    {
        // don't care
    }

    public string VisibleName()
    {
        return "event clock";
    }

    public void GotChuck( ChuckInstance chuck )
    {
        // don't care
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        // don't care
    }


    public void SizeChanged( float newSize )
    {
        // don't care
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // no state to clone
    }


    public float[] SerializeFloatParams( int version )
    {
        // nothing to store
        return LanguageObject.noFloatParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // nothing to store
        return LanguageObject.noIntParams;
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
        // nothing to load
    }
}
