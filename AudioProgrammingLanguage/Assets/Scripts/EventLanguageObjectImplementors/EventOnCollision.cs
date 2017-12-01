using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
[RequireComponent(typeof(EventNotifyController))]
public class EventOnCollision : MonoBehaviour , IEventLanguageObjectEmitter , IEventNotifyResponder
{
    private EventNotifyController myCollisionEventNotifier;

    private ChuckSubInstance myChuck;
    private string myTriggerEvent;
    private int myNumNumberChildren = 0;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // object init
        myCollisionEventNotifier = GetComponent<EventNotifyController>();

        // chuck init
        ChuckSubInstance theChuck = chuck;
        myChuck = theChuck;
        myTriggerEvent = theChuck.GetUniqueVariableName();

        theChuck.RunCode( string.Format( @"
            external Event {0};
            ", myTriggerEvent    
        ));
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        myChuck = null;
    }

    public string ExternalEventSource()
    {
        return myTriggerEvent;
    }

    public void RespondToEvent( float intensity )
    {
        // TODO: intensity?
        myChuck.BroadcastEvent( myTriggerEvent );
    }

    public void ShowEmit()
    {
        // TODO: change the block's appearance when it processes a collision
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        // no connection
        return "";
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
        if( other.GetComponent<DataReporter>() != null ||
            other is EventLanguageObject )
        {
            return true;
        }
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // is it a data reporter?
        if( child.GetComponent<DataReporter>() != null )
        {
            // connect the event collision event notify system into the Chuck EventLanguageObject system
            child.GetComponent<EventNotifyController>().AddListener( myCollisionEventNotifier );
        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // is it a data reporter?
        if( child.GetComponent<DataReporter>() != null )
        {
            // disconnect the event collision event notify system from the Chuck EventLanguageObject system
            child.GetComponent<EventNotifyController>().RemoveListener( myCollisionEventNotifier );
        }
    }

    public string VisibleName()
    {
        return "on collision";
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
