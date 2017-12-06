using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
[RequireComponent(typeof(EventNotifyController))]
public class EventOnCollision : MonoBehaviour , IEventLanguageObjectEmitter , IEventNotifyResponder
{
    public MeshRenderer myBox;

    private EventNotifyController myCollisionEventNotifier;

    private ChuckSubInstance myChuck;
    private string myTriggerEvent;
    private int myNumNumberChildren = 0;

    private int animationFrames = 16;
    private int animationFramesLeft = 0;
    private float targetHue;

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
        // change the block's appearance when it processes a collision
        animationFramesLeft = animationFrames;
        //Debug.Log("frames left! " + animationFramesLeft );
        targetHue = UnityEngine.Random.Range( 0.0f, 1.0f );
    }

    private void Update()
    {
        // set color according to recent collision
        if( animationFramesLeft >= 0 )
        {
            //Debug.Log(string.Format("hue {0}, saturation {1}", targetHue, animationFramesLeft * 1.0f / animationFrames ));
            myBox.material.color = Color.HSVToRGB(
                targetHue,
                animationFramesLeft * 1.0f / animationFrames,
                1.0f
            );
            animationFramesLeft--;
        }
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
