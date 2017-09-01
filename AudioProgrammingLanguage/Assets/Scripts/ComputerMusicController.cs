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

    private void Update()
    {

    }

    public ChuckInstance GetChuck()
    {
        return GetComponent<LanguageObject>().GetChuck();
    }

    public bool AcceptableChild( LanguageObject other, Collider mine )
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

    public void NewChild(LanguageObject child, Collider mine)
    {
        // don't care
    }

    public void ChildDisconnected(LanguageObject child)
    {
        // don't care
    }
}
