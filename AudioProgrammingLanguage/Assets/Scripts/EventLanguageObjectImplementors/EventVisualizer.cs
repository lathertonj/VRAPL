using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventVisualizer : MonoBehaviour , IEventLanguageObjectListener {

    public MeshRenderer myRenderer;
    
    public void TickDoAction()
    {
        // change color
        // NOTE: this will not be called unless the clock is a child of something else
        myRenderer.material.color = UnityEngine.Random.ColorHSV();
    }

    public void NewListenEvent( ChuckInstance theChuck, string incomingEvent )
    {
        // don't care
    }

    public void LosingListenEvent( ChuckInstance theChuck, string losingEvent)
    {
        // don't care
    }
    
    public bool AcceptableChild( LanguageObject other )
    {
        if( other.GetComponent<EventLanguageObject>() != null )
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

    public string InputConnection()
    {
        // have no connection
        return "";
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
