using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
public class FunctionOutputController : MonoBehaviour , ILanguageObjectListener
{

    public Renderer myBox;
    public TextMesh myText;

    // TODO: delete?
    public FunctionController myFunction;

    private string myStorageClass;
    private string myExitEvent;
    private LanguageObject myLO;
    private ChuckSubInstance myChuck;

    private int numChildren;

    // Use this for initialization
    public void InitLanguageObject( ChuckSubInstance chuck )
    {
	    myLO = GetComponent<LanguageObject>();	
        myChuck = chuck;

        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        chuck.RunCode( string.Format( @"
            external Event {1};
            public class {0} 
            {{
                static Gain @ myGain;
            }}

            Gain g @=> {0}.myGain;

            {1} => now;

        ", myStorageClass, myExitEvent ));
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
        // don't care -- won't ever have a parent
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        // don't care -- won't ever have a parent
    }
    
    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        if( other.GetComponent<SoundProducer>() != null )
        {
            return true;
        }

        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        numChildren++;
        if( numChildren == 1 )
        {
            SwitchColors();
        }

        LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        numChildren--;
        if( numChildren == 0 )
        {
            SwitchColors();
        }

        LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
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
