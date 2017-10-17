using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class MicrophoneController : MonoBehaviour {

    private static MicrophoneController theMic;
    private static string theFile = "";
    private static int srate;

    public static void StartRecording( string filename, float time )
    {
        // check if I am the One
        if( theFile != "" )
        {
            Debug.LogError( "can't record more than one thing at once!" );
            return;
        }

        // start recording
        theMic._StartRecording( time );

        // store
        theFile = filename;
    }

    public static void StopRecording( string filename )
    {
        // check if I am the One
        if( filename != theFile )
        {
            Debug.LogError( "asking to save a different file than when starting to record!" );
            return;
        }

        // stop recording and save
        theMic._StopRecording( filename );

        // reset
        theFile = "";
    }



    AudioClip myAudioClip;
    string myDevice;
    
    // Use this for initialization
	void Start () {
        // singleton
        if( theMic == null )
        {
            theMic = this;
            srate = AudioSettings.GetConfiguration().sampleRate;
        }
        else if( theMic != this )
        {
            Destroy( gameObject );
        }

        foreach( string device in Microphone.devices )
        {
            Debug.Log( device );
			if( device.Contains( "HTC Vive" ) )
            {
                Debug.Log( "Using microphone: " + device );
                myDevice = device;
                break;
            }
        }
        
	}
	
    private void _StartRecording( float time )
    {
        myAudioClip = Microphone.Start( myDevice, false, (int) ( time + 0.5f ), srate );
    }
    
    private void _StopRecording( string filename )
    {
        int endSamplePosition = Microphone.GetPosition( myDevice );
        Microphone.End( myDevice );

        // only save the part up until where the recording was ended
        float[] originalData = new float[myAudioClip.samples];
        float[] clippedData = new float[endSamplePosition];
        
        // copy data
        myAudioClip.GetData( originalData, 0 );
        Array.Copy( originalData, clippedData, clippedData.Length - 1 );

        // insert into new audio clip
        AudioClip shorterAudioClip = AudioClip.Create( "comment", endSamplePosition, 1, srate, false );
        shorterAudioClip.SetData( clippedData, 0 );

        // write to disk
        SavWav.Save( filename, shorterAudioClip );
    }
}
