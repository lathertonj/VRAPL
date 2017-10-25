using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventWait : MonoBehaviour , IEventLanguageObjectListener , IEventLanguageObjectEmitter {

    public MeshRenderer myRenderer;

    private string myStorageClass;
    private string myOutgoingTriggerEvent;
    private string myExitEvent;

    public void StartEmitTrigger() 
    {
        ChuckInstance theChuck = TheChuck.Instance;
        myStorageClass = theChuck.GetUniqueVariableName();
        myOutgoingTriggerEvent = theChuck.GetUniqueVariableName();
        myExitEvent = theChuck.GetUniqueVariableName();

        theChuck.RunCode( string.Format( @"
            external Event {1};
            external Event {2};

            public class {0}
            {{
                static Event @ myIncomingTriggerEvent;
                static Shred @ myCurrentShred;

                static Gain @ myGain;
                static Step @ myDefaultValue;

                fun static void BroadcastEvents()
                {{
                    while( true )
                    {{
                        {0}.myIncomingTriggerEvent => now;
                        {0}.myGain.last() => float secTimeToWait;
                        Math.max( secTimeToWait, 0.0001 ) => secTimeToWait;
                        secTimeToWait::second => now;
                        {2}.broadcast();
                    }}
                }}
            }}
            
            Event e @=> {0}.myIncomingTriggerEvent;


            Gain g @=> {0}.myGain;
            Step s @=> {0}.myDefaultValue;
            0.5 => {0}.myDefaultValue.next;
            {0}.myDefaultValue => {0}.myGain => blackhole;

            // broadcast
            spork ~ {0}.BroadcastEvents() @=> {0}.myCurrentShred;

            // wait until told to exit
            {1} => now;

            ", myStorageClass, myExitEvent, myOutgoingTriggerEvent    
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

    public void NewListenEvent( ChuckInstance theChuck, string incomingEvent )
    {
        // listen for the new event
        theChuck.RunCode( string.Format( @"
            external Event {1};
            // broadcast
            spork ~ {0}.BroadcastEvents() @=> {0}.myCurrentShred;
        ", myStorageClass, incomingEvent ));
    }

    public void LosingListenEvent( ChuckInstance theChuck, string losingEvent )
    {
        // exit the shred that is listening to the old event
        theChuck.RunCode( string.Format( @"
            {0}.myCurrentShred.exit();
        ", myStorageClass ));
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
        return "trigger clock";
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
