using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(SoundProducer))]
public class FunctionInputController : MonoBehaviour , ILanguageObjectListener
{
    public FunctionController myFunction;

    public Renderer myBox;
    public TextMesh myText;

    private ChuckSubInstance myChuck = null;
    private string myStorageClass;
    private string myExitEvent;
    
    private LanguageObject myLO;
    private ILanguageObjectListener myParent;

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
        return false;
    }

    public void NewParent( LanguageObject parent )
    {
        ILanguageObjectListener newParent = (ILanguageObjectListener) parent.GetComponent(typeof(ILanguageObjectListener));
        if( newParent != null )
        {
            SwitchColors();
            myParent = newParent;
        }
        
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        ILanguageObjectListener losingParent = (ILanguageObjectListener) parent.GetComponent(typeof(ILanguageObjectListener));
        if( losingParent == myParent )
        {
            SwitchColors();
            myParent = null;
        }
    }
    
    public void NewChild( LanguageObject child )
    {
        // don't care
    }

    public void ChildDisconnected( LanguageObject child )
    {
        // don't care
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

        ", myStorageClass, myExitEvent, myParent.InputConnection( myLO ) ));

        // store chuck before telling function to pass on the message
        // because the upcoming ugens will double check that this input has a chuck
        myChuck = chuck;
        // tell function that all its non-param children have a chuck now
        myFunction.TellUgenChildrenGotChuck( chuck );

    }

    public void LosingChuck( ChuckSubInstance chuck )
    {
        // tell children losing chuck before I set mychuck to null
        // because children will check whether I have chuck
        // if I don't have chuck, they will assume I never had chuck in the first place
        // and then not undo their own chucks
        myFunction.TellUgenChildrenLosingChuck( chuck );
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
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
