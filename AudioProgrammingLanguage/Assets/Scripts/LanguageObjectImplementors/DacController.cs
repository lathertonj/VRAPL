using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DacController : MonoBehaviour , ILanguageObjectListener , IControllerInputAcceptor
{
    public MeshRenderer[] myShapes;
    public TextMesh myText;

    private Color darkColor;
    private Color lightColor;

    private bool myEnabled = true;

    public void NewParent(LanguageObject parent)
    {
        // at the moment, dacs cannot be the children of anything.
    }

    public void ParentDisconnected(LanguageObject parent)
    {
        // at the moment, dacs cannot be the children of anything
    }

    public bool AcceptableChild( LanguageObject other )
    {
        // only accept things that can make sound
        if( other.GetComponent<SoundProducer>() != null )
        {
            return true;
        }

        return false;
    }

    // Use this for initialization
    void Awake() {
		darkColor = myText.color;
        lightColor = myShapes[0].material.color;
        SetColors();
	}
	
	private void SetColors()
    {
        Color bodyColor, textColor;
        if( myEnabled )
        {
            bodyColor = darkColor;
            textColor = lightColor;
        }
        else
        {
            bodyColor = lightColor;
            textColor = darkColor;
        }
        // set
        myText.color = textColor;
        foreach( MeshRenderer m in myShapes )
        {
            m.material.color = bodyColor;
        }
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return "dac";
    }

    public void GotChuck( ChuckSubInstance chuck )
    {
        // don't care
    }

    public void LosingChuck( ChuckSubInstance chuck )
    {
        // don't care
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public void NewChild( LanguageObject child )
    {
        // don't care
    }

    public void ChildDisconnected(LanguageObject child)
    {
        // don't care
    }

    public void TouchpadDown()
    {
        myEnabled = !myEnabled;
        SetColors();
        
        if( !myEnabled )
        {
            GetComponent<LanguageObject>().TellChildrenLosingChuck( GetComponent<ChuckSubInstance>() );
        }

        GetComponent<ChuckSubInstance>().SetRunning( myEnabled );

        if( myEnabled )
        {
            GetComponent<LanguageObject>().TellChildrenHaveNewChuck( GetComponent<ChuckSubInstance>() );
        }
    }

    public void TouchpadUp()
    {
        // don't care
    }

    public void TouchpadAxis(Vector2 pos)
    {
        // don't care
    }

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public bool IsEnabled()
    {
        return myEnabled;
    }

    public string VisibleName()
    {
        return myText.text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        DacController other = original.GetComponent<DacController>();
        if( myEnabled != other.myEnabled )
        {
            // simulate touchpad down
            TouchpadDown();
        }
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // no string params
        return LanguageObject.noStringParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // whether enabled
        return new int [] { myEnabled ? 1 : 0 };
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public object[] SerializeObjectParams( int version )
    {
        // no object params
        return LanguageObject.noObjectParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams )
    {
        // whether enabled
        myEnabled = ( intParams[0] != 0 );
        SetColors();
        GetComponent<ChuckSubInstance>().SetRunning( myEnabled );
    }
}
