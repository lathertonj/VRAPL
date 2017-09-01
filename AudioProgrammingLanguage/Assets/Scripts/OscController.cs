using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
public class OscController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor
{
    public GameObject myText;
    public GameObject myShape;
    public string myOscillatorType;

    private string myStorageClass;
    private string myExitEvent;
    private LanguageObject myParent = null;
    ILanguageObjectListener myParentListener = null;
    private string[] myAcceptableParams;
    private Dictionary<string, int> numParamConnections;

    private float myDefaultFrequency = 440.0f;

    void Start () {
        myAcceptableParams = new string[] { "freq", "gain" };
        numParamConnections = new Dictionary<string, int>();
        for( int i = 0; i < myAcceptableParams.Length; i++ ) { 
            numParamConnections[myAcceptableParams[i]] = 0; 
        }
	}

    private void Update()
    {
        // divide frequency by increase in size
        // min frequency for a frequency set in this way is 20
        myDefaultFrequency = Mathf.Max( 440.0f / ( 1 + 2 * ( GetComponent<MovableController>().myScale - 1 ) ), 20.0f );

        // if we have a chuck, set the default frequency
        if( GetChuck() != null )
        {
            GetChuck().RunCode(string.Format(
                "{1} => {0}.myDefaultFreq.next;", myStorageClass, myDefaultFrequency
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
        return myAcceptableParams;
    }

    public void ConnectParam( string param, string var )
    {
        if( !numParamConnections.ContainsKey( param ) )
        {
            return;
        }
        numParamConnections[param]++;
        
        if( param == myAcceptableParams[0] )
        {
            // freq
            GetChuck().RunCode(string.Format(
                "{1} => {0}.myFreq;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                GetChuck().RunCode(string.Format(
                    "0 => {0}.myDefaultFreq.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[1] )
        {
            // gain
            GetChuck().RunCode(string.Format(
               "{1} => {0}.myGain;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                GetChuck().RunCode(string.Format(
                    "0 => {0}.myDefaultGain.gain;", myStorageClass
                ));
            }
        }
    }

    public void DisconnectParam( string param, string var )
    {
        if( !numParamConnections.ContainsKey( param ) )
        {
            return;
        }
        numParamConnections[param]--;
        
        if( param == myAcceptableParams[0] )
        {
            // freq
            GetChuck().RunCode(string.Format(
                "{1} =< {0}.myFreq;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                GetChuck().RunCode(string.Format(
                    "1 => {0}.myDefaultFreq.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[1] )
        {
            // gain
            GetChuck().RunCode(string.Format(
               "{1} =< {0}.myGain;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                GetChuck().RunCode(string.Format(
                    "1 => {0}.myDefaultGain.gain;", myStorageClass
                ));
            }
        }
    }

    private void SwitchColors()
    {
        Color tempColor = myText.GetComponent<TextMesh>().color;
        myText.GetComponent<TextMesh>().color = myShape.GetComponent<Renderer>().material.color;
        myShape.GetComponent<Renderer>().material.color = tempColor;
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

        chuck.RunCode(string.Format(@"
            external Event {1};
            public class {0}
            {{
                static {3} @ myOsc;
                static Gain @ myGain;
                static Gain @ myFreq;
                static Step @ myDefaultGain;
                static Step @ myDefaultFreq;
            }}
            {3} o @=> {0}.myOsc;
            Gain g1 @=> {0}.myGain;
            Gain g2 @=> {0}.myFreq;
            Step s1 @=> {0}.myDefaultGain;
            Step s2 @=> {0}.myDefaultFreq;
            220 => {0}.myDefaultFreq.next;
            1 => {0}.myDefaultGain.next;
            {0}.myGain => blackhole;
            {0}.myFreq => blackhole;
            {0}.myDefaultGain => blackhole;
            {0}.myDefaultFreq => blackhole;

            fun void listenForGainChanges()
            {{
                while( true )
                {{
                    {0}.myGain.last() + {0}.myDefaultGain.last() => {0}.myOsc.gain;
                    1::ms => now;
                }}
            }}

            fun void listenForFreqChanges()
            {{
                while( true )
                {{
                    {0}.myFreq.last() + {0}.myDefaultFreq.last() => {0}.myOsc.freq;
                    1::ms => now;
                }}
            }}

            spork ~ listenForGainChanges();
            spork ~ listenForFreqChanges();

            {0}.myOsc => {2};

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent, connectMyOscTo, myOscillatorType ));
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
}
