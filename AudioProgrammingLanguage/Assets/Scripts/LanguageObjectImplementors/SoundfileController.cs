﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(EventNotifyController))]
[RequireComponent(typeof(SoundProducer))]
public class SoundfileController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor , IEventNotifyResponder , IControllerInputAcceptor
{
    public MeshRenderer[] myShapes;
    public TextMesh myText;

    private string[] myAcceptableParams;
    private Dictionary<string, int> numParamConnections;


    private string myStorageClass;
    private string myExitEvent;

    private string myFilename;
    private List<string> myFilenames;
    private int myFilenameIndex;

    private Vector2 lastAxis;

    private ChuckSubInstance myChuck = null;

    private ILanguageObjectListener myParent;
    private LanguageObject myLO;

    private Dictionary<EventNotifyController, bool> myNotifiers;

    // Use this for initialization
    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myLO = GetComponent<LanguageObject>();
		myAcceptableParams = new string[] { "rate", "gain" };
        numParamConnections = new Dictionary<string, int>();
        for( int i = 0; i < myAcceptableParams.Length; i++ )
        {
            numParamConnections[myAcceptableParams[i]] = 0;
        }
        myNotifiers = new Dictionary<EventNotifyController, bool>();

        // look up all filenames in StreamingAssets and populate myFilenames with the ones that are wav files
        myFilenames = new List<string>();
        DirectoryInfo dir = new DirectoryInfo( Application.streamingAssetsPath );
        FileInfo[] wavFiles = dir.GetFiles("*.wav");
        foreach( FileInfo f in wavFiles )
        {
            myFilenames.Add( f.Name );
        }

        myFilenameIndex = 0;
        myFilename = myFilenames[myFilenameIndex];
        myText.text = myFilename;

        lastAxis = Vector2.zero;

        // init chuck
        myChuck = chuck;

        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        string initCode = string.Format( @"
            global Event {1};
            public class {0}
            {{
                static Gain @ myOutput;

                static Gain @ myRate;
                static Step @ myDefaultRate;
                static Gain @ myGain;
                static Step @ myDefaultGain;
                
            }}
            Gain g @=> {0}.myOutput;

            Gain g1 @=> {0}.myRate;
            Gain g2 @=> {0}.myGain;

            Step s1 @=> {0}.myDefaultRate;
            Step s2 @=> {0}.myDefaultGain;

            1 => {0}.myDefaultRate.next;
            1 => {0}.myDefaultGain.next;
            
            {0}.myDefaultRate => {0}.myRate => blackhole;
            {0}.myDefaultGain => {0}.myGain => blackhole;

            // wait until told to exit
            {1} => now;
            ", myStorageClass, myExitEvent
        );

        chuck.RunCode( initCode );
	}

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public void ParentConnected( LanguageObject parent,ILanguageObjectListener parentListener )
    {
        SwitchColors();
        myParent = parentListener;
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        if( myParent == parentListener )
        {
            SwitchColors();
            myParent = null;
        }
    }

    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        if( other.GetComponent<ParamController>() != null )
        {
            return true;
        }
        else if( other.GetComponent<EventNotifyController>() != null )
        {
            return true;
        }
        return false;
    }
    
    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // neither ParamController nor EventNotifyController needs to be hooked up to me!
        EventNotifyController nc = child.GetComponent<EventNotifyController>();
        if( nc != null )
        {
            myNotifiers[nc] = true;
            nc.AddListener( GetComponent<EventNotifyController>() );
        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // neither ParamController nor EventNotifyController needs to be unhooked from me!
        EventNotifyController nc = child.GetComponent<EventNotifyController>();
        if( nc != null && myNotifiers.ContainsKey( nc ) )
        {
            nc.RemoveListener( GetComponent<EventNotifyController>() );
            myNotifiers.Remove( nc );
        }
    }
	
	void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShapes[0].material.color;
        foreach( MeshRenderer r in myShapes )
        {
            r.material.color = temp;
        }
    }

    public void RespondToEvent( float intensity )
    {
        if( myChuck != null )
        {
            myChuck.RunCode( string.Format( @"
                SndBuf s => {0}.myOutput;
                me.dir() + ""{1}"" => s.read;
                {0}.myRate.last() => s.rate;
                {0}.myGain.last() * {2:0.000} => s.gain;

                s.length() / s.rate() => now;
                s =< {0}.myOutput;
            ", myStorageClass, myFilename, intensity ) );
        }
    }

    public string[] AcceptableParams()
    {
        return myAcceptableParams;
    }

    public void ConnectParam(string param, string var)
    {
        if( !numParamConnections.ContainsKey( param ) )
        {
            return;
        }
        numParamConnections[param]++;
        
        if( param == myAcceptableParams[0] )
        {
            // rate
            myChuck.RunCode(string.Format(
                "{1} => {0}.myRate;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultRate.gain;", myStorageClass
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

    public void DisconnectParam(string param, string var)
    {
        if( !numParamConnections.ContainsKey( param ) )
        {
            return;
        }
        numParamConnections[param]--;
        
        if( param == myAcceptableParams[0] )
        {
            // rate
            myChuck.RunCode(string.Format(
                "{1} =< {0}.myRate;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultRate.gain;", myStorageClass
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
        return string.Format( "{0}.myOutput", myStorageClass );
    }

    public void TouchpadDown()
    {
        if( lastAxis.y > 0 )
        {
            // change filename
            myFilenameIndex++;
            myFilenameIndex %= myFilenames.Count;
            myFilename = myFilenames[myFilenameIndex];
            myText.text = myFilename;
        }
        else if( lastAxis.y < 0 )
        {
            // play it
            RespondToEvent( 1.0f );
        }
    }

    public void TouchpadUp()
    {
        // don't care
    }

    public void TouchpadTransform( Transform touchpad )
    {
        // don't care
    }

    public void TouchpadAxis( Vector2 pos )
    {
        lastAxis = pos;
    }
    
    public string VisibleName()
    {
        return myText.text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        SoundfileController other = original.GetComponent< SoundfileController >();

        // simulate button presses until the state matches
        lastAxis.y = 1;
        while( myFilenameIndex != other.myFilenameIndex )
        {
            TouchpadDown();
        }
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // store soundfile name
        return new string[] { myFilename };
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
        // try to find the file
        // simulate button presses until the state matches
        lastAxis.y = 1;
        while( myFilename != stringParams[0] )
        {
            TouchpadDown();
            if( myFilenameIndex == 0 )
            {
                // wrapped around again; give up
                return;
            }
        }
    }
}
