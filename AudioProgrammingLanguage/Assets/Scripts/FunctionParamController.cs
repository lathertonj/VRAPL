using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(NumberProducer))]
public class FunctionParamController : MonoBehaviour , ILanguageObjectListener
{

    public MeshRenderer myShape;
    public TextMesh myText;

    public ParamController myParent;

    private string myStorageClass;
    private string myExitEvent;

    public FunctionController myFunction;

    // Use this for initialization
    void Awake () {
        if( myFunction == null )
        {
		    myFunction = TheRoom.GetCurrentFunction().GetComponent<FunctionController>();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShape.material.color;
        myShape.material.color = temp;
    }

    public bool AcceptableChild( LanguageObject other )
    {
        return false;
    }

    public void NewParent( LanguageObject parent )
    {
        ParamController newParam = parent.GetComponent<ParamController>();
        if( newParam != null )
        {
            myParent = newParam;
            SwitchColors();
            myFunction.AddParam( this, myParent.GetParamName() );
        }
    }
    
    public void ParentDisconnected( LanguageObject parent )
    {
        ParamController losingParam = parent.GetComponent<ParamController>();
        if( losingParam == myParent )
        {
            myParent = null;
            SwitchColors();
            myFunction.RemoveParam( this );
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

    public void GotChuck( ChuckInstance chuck )
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

        ", myStorageClass, myExitEvent, myParent.InputConnection() ));
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        chuck.RunCode( string.Format(@"{0} =< {1};", OutputConnection(), myParent.InputConnection() ) );
        chuck.BroadcastEvent( myExitEvent );
    }
    
    public string InputConnection()
    {
        return string.Format( @"{0}.myGain", myStorageClass );
    }
    
    public string OutputConnection()
    {
        return InputConnection();
    }

    public string VisibleName()
    {
        return "input param";
    }

    public void CloneYourselfFrom(LanguageObject original, LanguageObject newParent)
    {
        // nothing to copy over
    }
}
