using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventRepeatImmediately : MonoBehaviour , IEventLanguageObjectListener , IEventLanguageObjectEmitter 
{
    // object
    public TextMesh myText;
    private long myDisplayNum;

    // chuck
    private ChuckSubInstance myChuck;
    private Chuck.IntCallback myNumRepeatsCallback;
    private LanguageObject myLO;
    private string myStorageClass;
    private string myOutgoingTriggerEvent;
    private string myOverallExitEvent;
    private string mySmallerExitEvent;
    private string myNumRepeats;
    private int myNumNumberChildren = 0;

    private void GetNumRepeats( long newVal )
    {
        myDisplayNum = newVal;
    }

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myLO = GetComponent<EventLanguageObject>();

        // init chuck
        myChuck = chuck;
        myStorageClass = myChuck.GetUniqueVariableName();
        myOutgoingTriggerEvent = myChuck.GetUniqueVariableName();
        myOverallExitEvent = myChuck.GetUniqueVariableName();
        myNumRepeats = myChuck.GetUniqueVariableName();
        myNumRepeatsCallback = Chuck.CreateGetIntCallback( GetNumRepeats );

        myChuck.RunCode( string.Format( @"
            external Event {1};
            external Event {2};
            external int {3};

            public class {0}
            {{
                static Gain @ myGain;
                static Step @ myDefaultValue;
            }}

            Gain g @=> {0}.myGain;
            Step s @=> {0}.myDefaultValue;
            3 => {0}.myDefaultValue.next;
            {0}.myDefaultValue => {0}.myGain => blackhole;

            fun void SetMyNumRepeats()
            {{
                while( true )
                {{
                    {0}.myGain.last() $ int => {3};
                    1::ms => now;
                }}
            }}
            spork ~ SetMyNumRepeats();

            // wait until told to exit
            {1} => now;

            ", myStorageClass, myOverallExitEvent, myOutgoingTriggerEvent, myNumRepeats  
        ));
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        myChuck.BroadcastEvent( myOverallExitEvent );
        myChuck = null;
    }

    private void Update()
    {
        // set my text with gotten value of mynumrepeats
        myText.text = string.Format("repeat immediately\n{0} times", myDisplayNum );
        // callback for next time
        myChuck.GetInt( myNumRepeats, myNumRepeatsCallback );
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
        // do nothing visually for now when I receive an event
    }

    public void FixedTickDoAction()
    {
        // do nothing visually during FixedUpdate when I receive an event
    }

    public void ShowEmit()
    {
        // do nothing visually for now when I emit an event
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
                    {0}.myGain.last() $ int => int numTimesToBroadcast;
                    repeat( numTimesToBroadcast )
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
        return "repeat immediately";
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
