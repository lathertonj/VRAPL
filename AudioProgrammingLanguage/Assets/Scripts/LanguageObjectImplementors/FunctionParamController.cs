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
    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        if( myFunction == null )
        {
            if( TheRoom.GetCurrentFunction() != null )
            {
		        myFunction = TheRoom.GetCurrentFunction().GetComponent<FunctionController>();
            }
        }
        myLO = GetComponent<LanguageObject>();

        // init chuck
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
	}

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
    }
	
	void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShape.material.color;
        myShape.material.color = temp;
    }

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        ParamController newParam = parent.GetComponent<ParamController>();
        if( newParam != null )
        {
            myParent = newParam;
            SwitchColors();
            myFunction.AddParam( this, myParent.GetParamName() );
        }
    }
    
    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
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

    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener otherListener )
    {
        // don't care -- no children
    }
    
    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener otherListener )
    {
        // don't care -- no children
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
