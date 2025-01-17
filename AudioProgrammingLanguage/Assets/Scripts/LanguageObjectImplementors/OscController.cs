﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
public class OscController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor
{
    public GameObject myText;
    public GameObject myShape;
    public string myOscillatorType;

    private ChuckSubInstance myChuck;
    private string myStorageClass;
    private string myExitEvent;
    private LanguageObject myParent = null;
    private LanguageObject myLO = null;
    ILanguageObjectListener myParentListener = null;
    private string[] myAcceptableParams;
    private Dictionary<string, int> numParamConnections;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myAcceptableParams = new string[] { "freq", "gain" };
        numParamConnections = new Dictionary<string, int>();
        for( int i = 0; i < myAcceptableParams.Length; i++ ) { 
            numParamConnections[myAcceptableParams[i]] = 0; 
        }
        myLO = GetComponent<LanguageObject>();

        // init chuck
        myChuck = chuck;
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        chuck.RunCode(string.Format(@"
            global Event {1};
            public class {0}
            {{
                static {2} @ myOsc;
                static Gain @ myGain;
                static Gain @ myFreq;
                static Step @ myDefaultGain;
                static Step @ myDefaultFreq;
            }}
            {2} o @=> {0}.myOsc;
            Gain g1 @=> {0}.myGain;
            Gain g2 @=> {0}.myFreq;
            Step s1 @=> {0}.myDefaultGain;
            Step s2 @=> {0}.myDefaultFreq;
            {3} => {0}.myDefaultFreq.next;
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

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent, myOscillatorType, GetMyDefaultFrequency() ));
	}

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    private float GetMyDefaultFrequency()
    {
        // divide frequency by increase in size
        // min frequency for a frequency set in this way is 20
        return Mathf.Max( 440.0f / ( 1 + 2 * ( GetComponent<MovableController>().GetScale() - 1 ) ), 20.0f );
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
        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // don't care -- param will connect itself to me
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // don't care -- param will connect itself to me
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
            myChuck.RunCode(string.Format(
                "{1} => {0}.myFreq;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultFreq.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[1] )
        {
            // gain
            myChuck.RunCode(string.Format(
               "{1} => {0}.myGain;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
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
            myChuck.RunCode(string.Format(
                "{1} =< {0}.myFreq;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultFreq.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[1] )
        {
            // gain
            myChuck.RunCode(string.Format(
               "{1} =< {0}.myGain;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
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

    

    public void SizeChanged( float newSize )
    {
        // if we have a chuck, set the default frequency
        if( myChuck != null )
        {
            myChuck.RunCode(string.Format(
                "{1} => {0}.myDefaultFreq.next;", myStorageClass, GetMyDefaultFrequency()
            ));
        }
    }
    
    public string VisibleName()
    {
        return myText.GetComponent<TextMesh>().text;
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
        // nothing to load from params (everything is in the prefab)
    }
}
