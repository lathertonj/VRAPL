using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventVisualizer : MonoBehaviour , IEventLanguageObjectListener {

    public MeshRenderer myRenderer;

    private void Awake()
    {
        myRenderer.material.color = Color.HSVToRGB( 1, 0.15f, 1 );
    }

    public void TickDoAction()
    {
        // change color when ticked
        // get hsv
        float h,s,v;
        Color.RGBToHSV( myRenderer.material.color, out h, out s, out v );
        // increment h
        h += 2 * 0.07134f;
        myRenderer.material.color = Color.HSVToRGB( h, s, v );
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
        return "event visualizer";
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
