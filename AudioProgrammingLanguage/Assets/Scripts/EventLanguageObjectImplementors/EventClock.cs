using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventClock : MonoBehaviour , IEventLanguageObjectListener {

    public MeshRenderer myRenderer;

    private EventLanguageObject myELO;
    private ChuckInstance myChuck;
    private Chuck.VoidCallback myTickCallback;
    private string myStorageClass;
    private string myTriggerEvent;
    private string myExitEvent;
    
    private int myNumTicks = 0;

    private void Awake()
    {
        // create tick callback
        myTickCallback = Chuck.CreateVoidCallback( TickIncCallback );

        // store
        myELO = GetComponent<EventLanguageObject>();
    }

    private void Start()
    {
        // manually trigger got chuck
        GotChuck( myELO.GetChuck() );
    }

    private void Update()
    {
        while( myNumTicks > 0 )
        {
            myNumTicks--;
            myELO.ProcessTickMessage();
        }
    }

    private void TickIncCallback()
    {
        myNumTicks++;
    }

    public bool TickDoAction()
    {
        // change color
        myRenderer.material.color = UnityEngine.Random.ColorHSV();

        // propagate the tick to children
        return true;
    }
    
    public bool AcceptableChild( LanguageObject other )
    {
        if( other.GetComponent<NumberProducer>() != null ||
            other.GetComponent<EventLanguageObject>() != null )
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
        myChuck = chuck;
        myStorageClass = myChuck.GetUniqueVariableName();
        myTriggerEvent = myChuck.GetUniqueVariableName();
        myExitEvent = myChuck.GetUniqueVariableName();

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

        myChuck.StartListeningForChuckEvent( myTriggerEvent, myTickCallback );
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        myChuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
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
