using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventWait : MonoBehaviour , IEventLanguageObjectListener , IEventLanguageObjectEmitter {

    public MeshRenderer myInputSphere;
    public MeshRenderer myOutputSphere;
    private int numFramesToShowInput = 0;
    private int numFramesToShowOutput = 0;

    private ChuckSubInstance myChuck;
    private LanguageObject myLO;
    private string myStorageClass;
    private string myOutgoingTriggerEvent;
    private string myOverallExitEvent;
    private string mySmallerExitEvent;
    private int myNumNumberChildren = 0;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myLO = GetComponent<EventLanguageObject>();

        // set colors
        myInputSphere.material.color = myOutputSphere.material.color = Color.blue;

        // set active
        myInputSphere.gameObject.SetActive( false );
        myOutputSphere.gameObject.SetActive( false );

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

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        myChuck.BroadcastEvent( myOverallExitEvent );
        myChuck = null;
    }

    private void Update()
    {
        // show input or disable it
        if( numFramesToShowInput > 0 )
        {
            numFramesToShowInput--;
        }
        else
        {
            myInputSphere.gameObject.SetActive( false );
        }

        // show output or disable it
        if( numFramesToShowOutput > 0 )
        {
            numFramesToShowOutput--;
        }
        else
        {
            myOutputSphere.gameObject.SetActive( false );
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
        // show my output sphere when I receive an event
        numFramesToShowInput = 10;
        myInputSphere.gameObject.SetActive( true );
    }

    public void FixedTickDoAction()
    {
        // do nothing during FixedUpdate when I receive an event
    }

    public void ShowEmit()
    {
        // show my output sphere when I emit an event
        numFramesToShowOutput = 10;
        myOutputSphere.gameObject.SetActive( true );
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

    public void LosingListenEvent( ChuckSubInstance theChuck, string losingEvent )
    {
        // exit the shred that is listening to the old event
        theChuck.BroadcastEvent( mySmallerExitEvent );
    }
    
    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care 
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care
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
            LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );

            myNumNumberChildren--;
            // is it the last number source? --> turn on my default
            if( myNumNumberChildren == 0 )
            {
                TheSubChuck.Instance.RunCode( string.Format( 
                    "1 => {0}.myDefaultValue.gain;", myStorageClass 
                ) );
            }
        }
    }

    public string VisibleName()
    {
        return "event wait";
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
