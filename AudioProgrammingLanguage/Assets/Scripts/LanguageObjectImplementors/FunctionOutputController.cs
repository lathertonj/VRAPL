﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
public class FunctionOutputController : MonoBehaviour , ILanguageObjectListener
{

    public Renderer myBox;
    public TextMesh myText;

    public FunctionController myFunction;

    private string myStorageClass;
    private string myExitEvent;
    private LanguageObject myLO;

    private int numChildren;

    // Use this for initialization
    void Awake()
    {
	    myLO = GetComponent<LanguageObject>();	
	}
	
	void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myBox.material.color;
        myBox.material.color = temp;
    }

    public bool AcceptableChild( LanguageObject other )
    {
        if( other.GetComponent<SoundProducer>() != null )
        {
            return true;
        }

        return false;
    }

    public void NewParent( LanguageObject parent )
    {
        // don't care
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        // don't care
    }
    
    public void NewChild( LanguageObject child )
    {
        numChildren++;
        if( numChildren == 1 )
        {
            SwitchColors();
        }
    }

    public void ChildDisconnected( LanguageObject child )
    {
        numChildren--;
        if( numChildren == 0 )
        {
            SwitchColors();
        }
    }

    public void GotChuck( ChuckSubInstance chuck )
    {
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        chuck.RunCode( string.Format( @"
            external Event {1};
            public class {0} 
            {{
                static Gain @ myGain;
            }}

            Gain g @=> {0}.myGain;
            {0}.myGain => {2};

            {1} => now;

        ", myStorageClass, myExitEvent, myFunction.GetFunctionParentConnection( myLO ) ));
    }

    public void LosingChuck(ChuckSubInstance chuck)
    {
        chuck.RunCode( string.Format(@"{0} =< {1};", OutputConnection(), myFunction.GetFunctionParentConnection( myLO ) ) );
        chuck.BroadcastEvent( myExitEvent );
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }
    
    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myGain", myStorageClass );
    }
    
    public string VisibleName()
    {
        return myText.text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // nothing to copy over
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // no string params
        return LanguageObject.noStringParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // no int params
        return LanguageObject.noIntParams;
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
        // nothing to load from params
    }
}
