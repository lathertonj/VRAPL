using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventLanguageObject))]
public class EventVisualizer : MonoBehaviour , IEventLanguageObjectListener {

    public MeshRenderer myRenderer;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myRenderer.material.color = Color.HSVToRGB( 1, 0.15f, 1 );

        // no chuck to init
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        // no chuck to cleanup
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

    public void FixedTickDoAction()
    {
        // do nothing during FixedUpdate when I receive an event
    }

    public void NewListenEvent( ChuckSubInstance theChuck, string incomingEvent )
    {
        // don't care
    }

    public void LosingListenEvent( ChuckSubInstance theChuck, string losingEvent)
    {
        // don't care
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
        if( other is EventLanguageObject )
        {
            return true;
        }
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // don't care
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // don't care
    }

    public string VisibleName()
    {
        return "event visualizer";
    }

    public void GotChuck( ChuckSubInstance chuck )
    {
        // don't care
    }

    public void LosingChuck( ChuckSubInstance chuck )
    {
        // don't care
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        // have no connection
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return "";
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
