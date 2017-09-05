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

    private string myStorageClass;
    private string myExitEvent;
    private LanguageObject myParent = null;
    ILanguageObjectListener myParentListener = null;
    private Dictionary<string, int> numParamConnections;

    private float myDefaultParam;

    void Start () {
        numParamConnections = new Dictionary<string, int>();
        for( int i = 0; i < myParams.Length; i++ ) { 
            numParamConnections[myParams[i]] = 0; 
        }
	}

    private void Update()
    {
        // divide param by increase in size
        // min param for a param set in this way is 20
        myDefaultParam = Mathf.Max( myParamDefaultValues[0] / ( 1 + 2 * ( GetComponent<MovableController>().myScale - 1 ) ), myDefaultParamMinimumValue );

        // if we have a chuck, set the default param
        if( GetChuck() != null )
        {
            GetChuck().RunCode(string.Format(
                "{0:0.000} => {1}.myDefault{2}.next;", myDefaultParam, myStorageClass, myParams[0]
            ));
        }
    }
    
    public bool AcceptableChild(LanguageObject other, Collider mine)
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

    public string InputConnection()
    {
        return string.Format("{0}.myOsc", myStorageClass);
    }

    public string OutputConnection()
    {
        return InputConnection();
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

        GetChuck().RunCode( string.Format(
            "{0} => {1}.my{2};", var, myStorageClass, param
        ));

        if( numParamConnections[param] == 1 )
        {
            // first connection: disable my default
            GetChuck().RunCode(string.Format(
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

        GetChuck().RunCode(string.Format(
            "{0} =< {1}.my{2};", var, myStorageClass, param    
        ));
        
        if( numParamConnections[param] == 0 )
        {
            // no more connections: enable my default
            GetChuck().RunCode(string.Format(
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

    public void NewParent( LanguageObject newParent )
    {
        myParent = newParent;
        myParentListener = (ILanguageObjectListener) myParent.GetComponent(typeof(ILanguageObjectListener));
        SwitchColors();
        
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        myParent = null;
        myParentListener = null;
        SwitchColors();
    }

    bool IsDac( LanguageObject other )
    {
        return ( other.GetComponent<ChuckInstance>() != null );
    }

    public ChuckInstance GetChuck()
    {
        return GetComponent<LanguageObject>().GetChuck();
    }

    public void GotChuck(ChuckInstance chuck)
    {
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();
        string connectMyOscTo = myParentListener.InputConnection();

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
            external Event {1};
            public class {0}
            {{
                static {3} @ myOsc;
                {4}
            }}

            {3} o @=> {0}.myOsc;
            {5}
            {6}
            {7}
            {8}

            {0}.myOsc.last();
            {2}.last();
            {0}.myOsc => {2};

            // wait until told to exit
            {1} => now;
            ", 
            myStorageClass, myExitEvent, connectMyOscTo, myType, 
            classDeclarations, oscDeclarations, defaultValues, blackholeConnections, functionListeners 
        );

        chuck.RunCode( oscCreation );
    }

    public void LosingChuck(ChuckInstance chuck)
    {
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
    
    public string VisibleName()
    {
        return myText.GetComponent<TextMesh>().text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // nothing to copy over
    }
}
