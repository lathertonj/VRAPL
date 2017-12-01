using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventIf : MonoBehaviour , IEventLanguageObjectListener , IEventLanguageObjectEmitter {

    public MeshRenderer myThenBlock;
    private int numFramesToShowThenBlock = 0;
    private Color originalThenBlockColor;

    private ChuckSubInstance myChuck;
    private LanguageObject myLO;
    private string myStorageClass;
    private string myOutgoingTriggerEvent;
    private string myOverallExitEvent;
    private string mySmallerExitEvent;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myLO = GetComponent<EventLanguageObject>();
        originalThenBlockColor = myThenBlock.material.color;

        // init chuck
        myChuck = chuck;
        myStorageClass = myChuck.GetUniqueVariableName();
        myOutgoingTriggerEvent = myChuck.GetUniqueVariableName();
        myOverallExitEvent = myChuck.GetUniqueVariableName();

        myChuck.RunCode( string.Format( @"
            external Event {1};
            external Event {2};

            public class {0}
            {{
                static Gain @ myGain;
            }}

            Gain g @=> {0}.myGain;
            {0}.myGain => blackhole;

            // wait until told to exit
            {1} => now;

            ", myStorageClass, myOverallExitEvent, myOutgoingTriggerEvent    
        ));
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        myChuck.BroadcastEvent( myOverallExitEvent );
        myChuck = null;
    }

    private void Update()
    {
        // disable then block color?
        if( numFramesToShowThenBlock > 0 )
        {
            numFramesToShowThenBlock--;
        }
        else
        {
            myThenBlock.material.color = originalThenBlockColor;
        }
    }

    public string ExternalEventSource()
    {
        return myOutgoingTriggerEvent;
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public void TickDoAction()
    {
        // do nothing when I receive an event
    }

    public void FixedTickDoAction()
    {
        // do nothing during FixedUpdate when I receive an event
    }

    public void ShowEmit()
    {
        // show my then block when I emit an event
        numFramesToShowThenBlock = 10;
        myThenBlock.material.color = Color.cyan;
    }

    public void NewListenEvent( ChuckSubInstance theChuck, string incomingEvent )
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
                    if( {0}.myGain.last() != 0 )
                    {{
                        {2}.broadcast();
                    }}
                }}
            }}
            // broadcast
            spork ~ BroadcastEvents();
            {3} => now;
        ", myStorageClass, incomingEvent, myOutgoingTriggerEvent, mySmallerExitEvent ));
    }

    public void LosingListenEvent( ChuckSubInstance theChuck, string losingEvent )
    {
        // exit the shred that is listening to the old event
        theChuck.BroadcastEvent( mySmallerExitEvent );
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
        if( child.GetComponent<NumberProducer>() != null )
        {
            LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( child.GetComponent<NumberProducer>() != null )
        {
            LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
        }
    }

    public string VisibleName()
    {
        return "event if";
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
