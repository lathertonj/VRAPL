using System;
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

    private ChuckInstance myChuck = null;

    private ILanguageObjectListener myParent;

    private Dictionary<EventNotifyController, bool> myNotifiers;

    // Use this for initialization
    void Awake () {
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
	}
	
	// Update is called once per frame
	void Update () {
		
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

    public bool AcceptableChild( LanguageObject other )
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

    public void NewParent( LanguageObject parent )
    {
        ILanguageObjectListener lo = (ILanguageObjectListener) parent.GetComponent( typeof( ILanguageObjectListener ) );
        if( lo != null )
        {
            SwitchColors();
            myParent = lo;
        }
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        ILanguageObjectListener lo = (ILanguageObjectListener) parent.GetComponent( typeof( ILanguageObjectListener ) );
        if( lo != null )
        {
            SwitchColors();
            myParent = null;
        }
    }

    public void NewChild( LanguageObject child )
    {
        EventNotifyController nc = child.GetComponent<EventNotifyController>();
        if( nc != null )
        {
            myNotifiers[nc] = true;
            nc.AddListener( GetComponent<EventNotifyController>() );
        }
    }

    public void ChildDisconnected( LanguageObject child )
    {
        EventNotifyController nc = child.GetComponent<EventNotifyController>();
        if( nc != null && myNotifiers.ContainsKey( nc ) )
        {
            nc.RemoveListener( GetComponent<EventNotifyController>() );
            myNotifiers.Remove( nc );
        }
    }

    public void GotChuck( ChuckInstance chuck )
    {
        myChuck = chuck;

        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        string initCode = string.Format( @"
            external Event {1};
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

            {0}.myOutput => {2};

            // wait until told to exit
            {1} => now;
            ", myStorageClass, myExitEvent, myParent.InputConnection()
        );

        chuck.RunCode( initCode );
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        if( myParent != null )
        {
            chuck.RunCode(string.Format("{0} =< {1};", OutputConnection(), myParent.InputConnection() ) );
        }

        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myOutput", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
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
}
