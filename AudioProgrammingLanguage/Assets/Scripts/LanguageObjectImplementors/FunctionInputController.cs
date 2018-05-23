using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(SoundProducer))]
public class FunctionInputController : MonoBehaviour , ILanguageObjectListener
{
    // TODO: delete?
    public FunctionController myFunction;

    public Renderer myBox;
    public TextMesh myText;

    private ChuckSubInstance myChuck = null;
    private string myStorageClass;
    private string myExitEvent;
    
    private LanguageObject myLO;
    private ILanguageObjectListener myParent;

    // Use this for initialization
    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // object init 
	    myLO = GetComponent<LanguageObject>();	

        // chuck init 
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        chuck.RunCode( string.Format( @"
            global Event {1};
            public class {0} 
            {{
                static Gain @ myGain;
            }}

            Gain g @=> {0}.myGain;

            {1} => now;

        ", myStorageClass, myExitEvent ));

        // store chuck
        myChuck = chuck;
	}

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }
	
	void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myBox.material.color;
        myBox.material.color = temp;
    }

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        SwitchColors();
        myParent = parentListener;
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        if( parentListener == myParent )
        {
            SwitchColors();
            myParent = null;
        }
    }
    
    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // don't care -- no children
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // don't care -- no children
    }

    public bool CurrentlyHaveChuck()
    {
        return myChuck != null;
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
