using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(SoundProducer))]
public class ComputerMusicController : MonoBehaviour , ILanguageObjectListener
{

    public GameObject myText;
    public GameObject myShape;

    private string myExitEvent;
    private ILanguageObjectListener myParent;

    private void SwitchColors()
    {
        Color tempColor = myText.GetComponent<TextMesh>().color;
        myText.GetComponent<TextMesh>().color = myShape.GetComponent<Renderer>().material.color;
        myShape.GetComponent<Renderer>().material.color = tempColor;
    }

    public void NewParent( LanguageObject newParent )
    {
        ILanguageObjectListener lo = (ILanguageObjectListener) newParent.GetComponent( typeof(ILanguageObjectListener) );
        if( lo != null )
        {
            SwitchColors();
            myParent = lo;
        }
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        ILanguageObjectListener lo = (ILanguageObjectListener) parent.GetComponent( typeof(ILanguageObjectListener) );
        if( lo == myParent )
        {
            SwitchColors();
            myParent = null;
        }
    }

    public bool AcceptableChild( LanguageObject other )
    {
        // computer music cannot have any children.
        return false;
    }

    public string InputConnection()
    {
        // nothing should be connecting to ComputerMusicController anyway
        return "";
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public string VisibleName()
    {
        return "computer music";
    }

    public void GotChuck( ChuckInstance chuck )
    {
        // get a variable name
        myExitEvent = chuck.GetUniqueVariableName();

        // run my script
        chuck.RunCode(string.Format(@"
            external Event {0};
            TriOsc foo => {1};
            fun void playMusic()
            {{
                while( true )
                {{
                    Math.random2f( 300, 1000 ) => foo.freq;
                    100::ms => now;
                }}
            }}
            spork ~ playMusic();

            {0} => now;
            foo =< dac;
            
        ", myExitEvent, myParent.InputConnection() ));
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        // Stop my script
        chuck.BroadcastEvent( myExitEvent );
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public void NewChild( LanguageObject child )
    {
        // don't care
    }

    public void ChildDisconnected( LanguageObject child )
    {
        // don't care
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // no state to copy
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
