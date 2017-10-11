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

    private bool enabled = true;

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
    void Start () {
		darkColor = myText.color;
        lightColor = myShapes[0].material.color;
        SetColors();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void SetColors()
    {
        Color bodyColor, textColor;
        if( enabled )
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

    public string InputConnection()
    {
        return "dac";
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public void GotChuck( ChuckInstance chuck )
    {
        // don't care
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        // don't care
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
        enabled = !enabled;
        SetColors();
        
        if( !enabled )
        {
            GetComponent<LanguageObject>().TellChildrenLosingChuck( GetComponent<ChuckInstance>() );
        }

        GetComponent<ChuckInstance>().SetRunning( enabled );

        if( enabled )
        {
            GetComponent<LanguageObject>().TellChildrenHaveNewChuck( GetComponent<ChuckInstance>() );
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

    public string VisibleName()
    {
        return myText.text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        DacController other = original.GetComponent<DacController>();
        if( enabled != other.enabled )
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
        return new int [] { enabled? 1 : 0 };
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, float[] floatParams )
    {
        // whether enabled
        if( enabled != ( intParams[0] != 0 ) )
        {
            // simulate touchpad down
            TouchpadDown();  
        }
    }
}
