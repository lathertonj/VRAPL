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
    private string myStorageClass;
    private ILanguageObjectListener myParent;
    private LanguageObject myLO;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myLO = GetComponent<LanguageObject>();

        // init chuck
        // get a variable name
        myExitEvent = chuck.GetUniqueVariableName();
        myStorageClass = chuck.GetUniqueVariableName();

        // run my script
        chuck.RunCode(string.Format(@"
            external Event {0};
            public class {1}
            {{
                static TriOsc @ myOsc;
            }}

            TriOsc foo @=> {1}.myOsc;
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
        ", myExitEvent, myStorageClass ));
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        // Stop my script
        chuck.BroadcastEvent( myExitEvent );
    }

    private void SwitchColors()
    {
        Color tempColor = myText.GetComponent<TextMesh>().color;
        myText.GetComponent<TextMesh>().color = myShape.GetComponent<Renderer>().material.color;
        myShape.GetComponent<Renderer>().material.color = tempColor;
    }

    public void ParentConnected( LanguageObject newParent, ILanguageObjectListener parentListener )
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
        // computer music cannot have any children.
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

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myOsc", myStorageClass );
    }

    public string VisibleName()
    {
        return "computer music";
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
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
