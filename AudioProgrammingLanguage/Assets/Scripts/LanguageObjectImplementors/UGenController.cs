using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(SoundProducer))]
public class UGenController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor
{
    public GameObject myText;
    public GameObject myShape;
    public string myType;
    public string[] myParams;
    public float[] myParamDefaultValues;
    public float myDefaultParamMinimumValue;
    public string shaderColorName = "";

    private ChuckSubInstance myChuck;
    private string myStorageClass;
    private string myExitEvent;
    private LanguageObject myParent = null;
    private LanguageObject myLO = null;
    ILanguageObjectListener myParentListener = null;
    private Dictionary<string, int> numParamConnections;

    public void InitLanguageObject( ChuckSubInstance chuck)
    {
        // init object
        numParamConnections = new Dictionary<string, int>();
        for( int i = 0; i < myParams.Length; i++ ) { 
            numParamConnections[myParams[i]] = 0; 
        }
        myLO = GetComponent<LanguageObject>();

        // init chuck
        myChuck = chuck;
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        string classDeclarations = "";
        string oscDeclarations = "";
        string defaultValues = "";
        string blackholeConnections = "";
        string functionListeners = "";

        for( int i = 0; i < myParams.Length; i++ )
        {
            classDeclarations += string.Format( "static Gain @ my{0}; static Step @ myDefault{0}; \n", myParams[i] );
            oscDeclarations += string.Format( "Gain g{2} @=> {0}.my{1}; Step s{2} @=> {0}.myDefault{1}; \n", myStorageClass, myParams[i], i );
            defaultValues += string.Format("{0} => {1}.myDefault{2}.next; \n", myParamDefaultValues[i], myStorageClass, myParams[i] );
            blackholeConnections += string.Format( "{0}.my{1} => blackhole; {0}.myDefault{1} => blackhole; \n", myStorageClass, myParams[i] );
            functionListeners += string.Format( @"
                fun void listenFor{1}Changes()
                {{
                    while( true )
                    {{
                        {0}.my{1}.last() + {0}.myDefault{1}.last() => {0}.myOsc.{1};
                        1::ms => now;
                    }}
                }}
                spork ~ listenFor{1}Changes();
            ", myStorageClass, myParams[i] );
        }

        string oscCreation = string.Format(@"
            global Event {1};
            public class {0}
            {{
                static {2} @ myOsc;
                {3}
            }}

            {2} o @=> {0}.myOsc;
            {4}
            {5}
            {6}
            {7}

            {0}.myOsc.last();
            
            // wait until told to exit
            {1} => now;
            ", 
            myStorageClass, myExitEvent, myType, 
            classDeclarations, oscDeclarations, defaultValues, blackholeConnections, functionListeners 
        );

        chuck.RunCode( oscCreation );
        SetMyDefaultParam();
	}

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    private float GetMyDefaultParam()
    {
        // divide param by increase in size
        // min param for a param set in this way is 20
        return Mathf.Max( 
            myParamDefaultValues[0] / 
                ( 1 + 2 * ( GetComponent<MovableController>().GetScale() - 1 ) ),
            myDefaultParamMinimumValue 
        ); 
    }

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        myParent = parent;
        myParentListener = parentListener;
        SwitchColors();
        
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        myParent = null;
        myParentListener = null;
        SwitchColors();
    }
    
    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        // allow params to be my child
        if( other.GetComponent<ParamController>() != null )
        {
            return true;
        }
        // allow other sound producers to connect to me
        else if( other.GetComponent<SoundProducer>() != null )
        {
            return true;
        }

        return false;
    }
        
    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( child.GetComponent<SoundProducer>() != null )
        {
            LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( child.GetComponent<SoundProducer>() != null )
        {
            LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
        }
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format("{0}.myOsc", myStorageClass);
    }

    public string[] AcceptableParams()
    {
        return myParams;
    }

    public void ConnectParam( string param, string var )
    {
        if( !numParamConnections.ContainsKey( param ) )
        {
            return;
        }
        numParamConnections[param]++;

        myChuck.RunCode( string.Format(
            "{0} => {1}.my{2};", var, myStorageClass, param
        ));

        if( numParamConnections[param] == 1 )
        {
            // first connection: disable my default
            myChuck.RunCode(string.Format(
                "0 => {0}.myDefault{1}.gain;", myStorageClass, param
            ));
        }
    }

    public void DisconnectParam( string param, string var )
    {
        if( !numParamConnections.ContainsKey( param ) )
        {
            return;
        }
        numParamConnections[param]--;

        myChuck.RunCode(string.Format(
            "{0} =< {1}.my{2};", var, myStorageClass, param    
        ));
        
        if( numParamConnections[param] == 0 )
        {
            // no more connections: enable my default
            myChuck.RunCode(string.Format(
                "1 => {0}.myDefault{1}.gain;", myStorageClass, param
            ));
        }
    }

    private void SwitchColors()
    {
        Color tempColor = myText.GetComponent<TextMesh>().color;
        if( shaderColorName == "" )
        {
            // basic shader
            myText.GetComponent<TextMesh>().color = myShape.GetComponent<Renderer>().material.color;
            myShape.GetComponent<Renderer>().material.color = tempColor;
        }
        else
        {
            // specified a color on the shader to switch with
            myText.GetComponent<TextMesh>().color = myShape.GetComponent<Renderer>().material.GetColor( shaderColorName );
            myShape.GetComponent<Renderer>().material.SetColor( shaderColorName, tempColor );
        }
    }

    public void SizeChanged( float newSize )
    {
        SetMyDefaultParam();
    }

    private void SetMyDefaultParam()
    {
        // if we have a chuck, set the default param
        if( myChuck != null )
        {
            myChuck.RunCode(string.Format(
                "{0:0.000} => {1}.myDefault{2}.next;", GetMyDefaultParam(), myStorageClass, myParams[0]
            ));
        }
    }
    
    public string VisibleName()
    {
        return myText.GetComponent<TextMesh>().text;
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
        // nothing to load from params -- all stored in prefab
    }
}
