using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventClock : MonoBehaviour , IEventLanguageObjectEmitter {

    public Transform myClockHand;
    public MeshRenderer myClockHandMesh;

    private LanguageObject myLO;
    private ChuckSubInstance myChuck;
    private string myStorageClass;
    private string myTriggerEvent;
    private string myExitEvent;
    private int myNumNumberChildren = 0;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // object init
        myClockHandMesh.material.color = Color.black;
        myLO = GetComponent<EventLanguageObject>();

        // chuck init
        ChuckSubInstance theChuck = chuck;
        myChuck = theChuck;
        myStorageClass = theChuck.GetUniqueVariableName();
        myTriggerEvent = theChuck.GetUniqueVariableName();
        myExitEvent = theChuck.GetUniqueVariableName();

        theChuck.RunCode( string.Format( @"
            global Event {1};
            global Event {2};

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

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        myChuck.BroadcastEvent( myExitEvent );
        myChuck = null;
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

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
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
        if( other.GetComponent<NumberProducer>() != null ||
            other is EventLanguageObject )
        {
            return true;
        }
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // is it a new number source?
        if( child.GetComponent<NumberProducer>() != null )
        {
            // add the child to my gain
            LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
            
            myNumNumberChildren++;
            // is it the first number source? --> turn off my default
            if( myNumNumberChildren == 1 )
            {
                myChuck.RunCode( string.Format( 
                    "0 => {0}.myDefaultValue.gain;", myStorageClass 
                ) );
            }

        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
       // is it a number source?
        if( child.GetComponent<NumberProducer>() != null )
        {
            myNumNumberChildren--;
            // is it the last number source? --> turn on my default
            if( myNumNumberChildren == 0 )
            {
                myChuck.RunCode( string.Format( 
                    "1 => {0}.myDefaultValue.gain;", myStorageClass 
                ) );
            }

            // remove the child from my gain
            LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
        }
    }

    public string VisibleName()
    {
        return "event clock";
    }

    public void GotChuck( ChuckSubInstance chuck )
    {
        // don't care
    }

    public void LosingChuck( ChuckSubInstance chuck )
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
