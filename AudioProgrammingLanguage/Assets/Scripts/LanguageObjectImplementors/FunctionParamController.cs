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
    private LanguageObject myLO;

    public FunctionController myFunction;

    // Use this for initialization
    void Awake()
    {
        if( myFunction == null )
        {
            if( TheRoom.GetCurrentFunction() != null )
            {
		        myFunction = TheRoom.GetCurrentFunction().GetComponent<FunctionController>();
            }
        }
        myLO = GetComponent<LanguageObject>();
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

        ", myStorageClass, myExitEvent, myParent.InputConnection( myLO ) ));
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        chuck.RunCode( string.Format(@"{0} =< {1};", OutputConnection(), myParent.InputConnection( myLO ) ) );
        chuck.BroadcastEvent( myExitEvent );
    }
    
    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }
    
    public string OutputConnection()
    {
        return string.Format( @"{0}.myGain", myStorageClass );
    }

    public string VisibleName()
    {
        return "input param";
    }

    public void CloneYourselfFrom(LanguageObject original, LanguageObject newParent)
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
