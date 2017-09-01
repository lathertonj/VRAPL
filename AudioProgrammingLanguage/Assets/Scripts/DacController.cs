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

    public bool AcceptableChild( LanguageObject other, Collider mine )
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

    public void NewChild(LanguageObject child, Collider mine)
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
}
