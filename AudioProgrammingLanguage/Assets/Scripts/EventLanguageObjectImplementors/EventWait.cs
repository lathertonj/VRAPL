using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventWait : MonoBehaviour , IEventLanguageObjectListener , IEventLanguageObjectEmitter {

    public MeshRenderer myRenderer;

    private string myStorageClass;
    private string myOutgoingTriggerEvent;
    private string myOverallExitEvent;
    private string mySmallerExitEvent;

    public void StartEmitTrigger() 
    {
        ChuckInstance theChuck = TheChuck.Instance;
        myStorageClass = theChuck.GetUniqueVariableName();
        myOutgoingTriggerEvent = theChuck.GetUniqueVariableName();
        myOverallExitEvent = theChuck.GetUniqueVariableName();

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
            0.5 => {0}.myDefaultValue.next;
            {0}.myDefaultValue => {0}.myGain => blackhole;

            // wait until told to exit
            {1} => now;

            ", myStorageClass, myOverallExitEvent, myOutgoingTriggerEvent    
        ));
    }

    public string ExternalEventSource()
    {
        return myOutgoingTriggerEvent;
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public void TickDoAction()
    {
        // don't do anything on an action
    }

    public void ShowEmit()
    {
        // don't show anything on emit
    }

    public void NewListenEvent( ChuckInstance theChuck, string incomingEvent )
    {
        // listen for the new event
        mySmallerExitEvent = theChuck.GetUniqueVariableName();
        theChuck.RunCode( string.Format( @"
            external Event {1};
            external Event {2};
            external Event {3};

            fun void BroadcastEvents()
            {{
                while( true )
                {{
                    {1} => now;
                    {0}.myGain.last() => float secTimeToWait;
                    Math.max( secTimeToWait, 0.0001 ) => secTimeToWait;
                    secTimeToWait::second => now;
                    {2}.broadcast();
                }}
            }}
            // broadcast
            spork ~ BroadcastEvents();
            {3} => now;
        ", myStorageClass, incomingEvent, myOutgoingTriggerEvent, mySmallerExitEvent ));
    }

    public void LosingListenEvent( ChuckInstance theChuck, string losingEvent )
    {
        // exit the shred that is listening to the old event
        theChuck.BroadcastEvent( mySmallerExitEvent );
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
        return "event wait";
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
