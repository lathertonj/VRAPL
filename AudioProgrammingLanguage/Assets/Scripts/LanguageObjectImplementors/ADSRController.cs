using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(EventNotifyController))]
[RequireComponent(typeof(SoundProducer))]
public class ADSRController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor , IEventNotifyResponder , IControllerInputAcceptor
{
    public MeshRenderer[] myShapes;
    public TextMesh myText;

    private string[] myAcceptableParams;
    //private string[] myParamDefaults;
    private Dictionary<string, int> numParamConnections;


    private string myStorageClass;
    private string myExitEvent;

    private ChuckInstance myChuck = null;

    private ILanguageObjectListener myParent;
    private LanguageObject myLO = null;

    private Dictionary<EventNotifyController, bool> myNotifiers;

    // Use this for initialization
    void Awake() {
		myAcceptableParams = new string[] { "attack time", "decay time", "sustain time", "sustain level", "release time" };
        //myParamDefaults = new string[] { "10::ms", "20::ms", "0.5::second", "0.5", "0.5::second" };
        numParamConnections = new Dictionary<string, int>();
        for( int i = 0; i < myAcceptableParams.Length; i++ )
        {
            numParamConnections[myAcceptableParams[i]] = 0;
        }
        myNotifiers = new Dictionary<EventNotifyController, bool>();
        myLO = GetComponent<LanguageObject>();
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
                {1} => ADSR a => {2};
                {3} => a.gain;
                a.set( ({0}.myAttackTime.last())::second, 
                       ({0}.myDecayTime.last())::second,
                        {0}.mySustainLevel.last(),
                       ({0}.myReleaseTime.last())::second );
                1 => a.keyOn;
                a.attackTime() + a.decayTime() + ({0}.mySustainTime.last())::second => now;
                1 => a.keyOff;
                a.releaseTime() => now;
                {1} => a;
                a =< {2};
            ", myStorageClass, this.InputConnection( null ), OutputConnection(), intensity
            ));
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
            // attack time
            myChuck.RunCode(string.Format(
                "{1} => {0}.myAttackTime;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultAttackTime.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[1] )
        {
            // decay time
            myChuck.RunCode(string.Format(
               "{1} => {0}.myDecayTime;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultDecayTime.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[2] )
        {
            // sustain time
            myChuck.RunCode(string.Format(
               "{1} => {0}.mySustainTime;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultSustainTime.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[3] )
        {
            // sustain level
            myChuck.RunCode(string.Format(
               "{1} => {0}.mySustainLevel;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultSustainLevel.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[4] )
        {
            // release time
            myChuck.RunCode(string.Format(
               "{1} => {0}.myReleaseTime;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 1 )
            {
                // first connection: disable my default
                myChuck.RunCode(string.Format(
                    "0 => {0}.myDefaultReleaseTime.gain;", myStorageClass
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
            // attack time
            myChuck.RunCode(string.Format(
                "{1} =< {0}.myAttackTime;", myStorageClass, var    
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultAttackTime.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[1] )
        {
            // decay time
            myChuck.RunCode(string.Format(
               "{1} =< {0}.myDecayTime;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultDecayTime.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[2] )
        {
            // sustain time
            myChuck.RunCode(string.Format(
               "{1} =< {0}.mySustainTime;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultSustainTime.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[3] )
        {
            // sustain level
            myChuck.RunCode(string.Format(
               "{1} =< {0}.mySustainLevel;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultSustainLevel.gain;", myStorageClass
                ));
            }
        }
        else if( param == myAcceptableParams[4] )
        {
            // release time
            myChuck.RunCode(string.Format(
               "{1} =< {0}.myReleaseTime;", myStorageClass, var 
            ));

            if( numParamConnections[param] == 0 )
            {
                // no more connections: enable my default
                myChuck.RunCode(string.Format(
                    "1 => {0}.myDefaultReleaseTime.gain;", myStorageClass
                ));
            }
        }
    }

    public bool AcceptableChild( LanguageObject other )
    {
        if( other.GetComponent<ParamController>() != null ||
            other.GetComponent<EventNotifyController>() != null ||
            other.GetComponent<SoundProducer>() != null )
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


        chuck.RunCode(string.Format(@"
            external Event {1};
            public class {0}
            {{
                static Gain @ myInput;
                static Gain @ myOutput;
                static dur myNoteLength;

                static Gain @ myAttackTime;
                static Step @ myDefaultAttackTime;
                static Gain @ myDecayTime;
                static Step @ myDefaultDecayTime;
                static Gain @ mySustainTime;
                static Step @ myDefaultSustainTime;
                static Gain @ mySustainLevel;
                static Step @ myDefaultSustainLevel;
                static Gain @ myReleaseTime;
                static Step @ myDefaultReleaseTime;
            }}
            Gain input @=> {0}.myInput;
            Gain output @=> {0}.myOutput;

            Gain g1 @=> {0}.myAttackTime;
            Gain g2 @=> {0}.myDecayTime;
            Gain g3 @=> {0}.mySustainTime;
            Gain g4 @=> {0}.mySustainLevel;
            Gain g5 @=> {0}.myReleaseTime;

            Step s1 @=> {0}.myDefaultAttackTime;
            Step s2 @=> {0}.myDefaultDecayTime;
            Step s3 @=> {0}.myDefaultSustainTime;
            Step s4 @=> {0}.myDefaultSustainLevel;
            Step s5 @=> {0}.myDefaultReleaseTime;

            0.01 => {0}.myDefaultAttackTime.next;
            0.02 => {0}.myDefaultDecayTime.next;
            0.5 => {0}.myDefaultSustainTime.next;
            0.5 => {0}.myDefaultSustainLevel.next;
            0.5 => {0}.myDefaultReleaseTime.next;

            {0}.myDefaultAttackTime => {0}.myAttackTime => blackhole;
            {0}.myDefaultDecayTime => {0}.myDecayTime => blackhole;
            {0}.myDefaultSustainTime => {0}.mySustainTime => blackhole;
            {0}.myDefaultSustainLevel => {0}.mySustainLevel => blackhole;
            {0}.myDefaultReleaseTime => {0}.myReleaseTime => blackhole;


            {0}.myOutput => {2};

            // wait until told to exit
            {1} => now;
            ", myStorageClass, myExitEvent, myParent.InputConnection( myLO )
        ));
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        if( myParent != null )
        {
            chuck.RunCode(string.Format("{0} =< {1};", OutputConnection(), myParent.InputConnection( myLO ) ) );
        }

        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return string.Format( "{0}.myInput", myStorageClass );
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myOutput", myStorageClass );
    }

    public string VisibleName()
    {
        return "adsr";
    }

    public void TouchpadDown()
    {
        RespondToEvent( 1.0f );
    }

    public void TouchpadUp()
    {
        // don't care
    }

    public void TouchpadAxis(Vector2 pos)
    {
        // don't care
    }

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // no settings to be copied over
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
