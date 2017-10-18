using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(LanguageObject))]
//[RequireComponent(typeof(SoundProducer))]
public class CommentController : MonoBehaviour, ILanguageObjectListener, IControllerInputAcceptor 
{
    public float maxCommentLengthSeconds = 20f;

    private static int commentCounter;
    private static string commentDir;

    public MeshRenderer[] myShapes;
    public TextMesh myText;
    private Color[] myShapesOriginalColors;

    private ILanguageObjectListener myParent;

    private ChuckInstance myChuck = null;
    private string myStorageClass;
    private string myExitEvent;


    private string myFilename;

    // recording
    private float touchpadPressTime;
    private bool touchpadHeld = false;
    private bool isPlayback = true;
    private bool haveRecordedOnce = false;


    // Use this for initialization
    void Awake()
    {
        commentDir = Application.persistentDataPath + "/audio/comments";
        Debug.Log( commentDir );
        // make sure exists
        Directory.CreateDirectory( commentDir );

        // store
        myShapesOriginalColors = new Color[myShapes.Length];
        for( int i = 0; i < myShapes.Length; i++ )
        {
            myShapesOriginalColors[i] = myShapes[i].material.color;
        }

        // get my name
        myFilename = CommentController.GetNextName();
	}
	
	// Update is called once per frame
	void Update () {
		// if touchpad being held down for more than 0.75 seconds
        if( touchpadHeld && isPlayback && Time.time - touchpadPressTime > 0.75f )
        {
            // no longer playback -- start recording
            isPlayback = false;

            MicrophoneController.StartRecording( myFilename, maxCommentLengthSeconds );

            // change color to red
            for( int i = 0; i < myShapes.Length; i++ )
            {
                myShapes[i].material.color = Color.red;
            }
        }
	}

    private static string GetNextName()
    {
        while( File.Exists( GetCurrentName() ) )
        {
            commentCounter++;
        }

        return GetCurrentName();
    }
    
    private static string GetCurrentName()
    {
        return string.Format("{0}/{1}.wav", commentDir, commentCounter );
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


    public bool AcceptableChild( LanguageObject other )
    {
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
        // don't care
    }

    public void ChildDisconnected( LanguageObject child )
    {
        // don't care
    }

    public void GotChuck( ChuckInstance chuck )
    {
        myChuck = chuck;

        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        myChuck.RunCode( string.Format(
            @"external Event {1};
            public class {0}
            {{
                static Gain @ myInput;
                static Gain @ myOutput;
            }}
            Gain input @=> {0}.myInput;
            Gain output @=> {0}.myOutput;


            // wait until told to exit
            {1} => now; 
            ", myStorageClass, myExitEvent
        ));
        
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myInput", myStorageClass );
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myOutput", myStorageClass );
    }

    public string VisibleName()
    {
        return "comment";
    }

    public void TouchpadDown()
    {
        touchpadPressTime = Time.time;
        touchpadHeld = true;
        // best guess
        isPlayback = true;
    }

    public void TouchpadUp()
    {
        touchpadHeld = false;
        if( isPlayback )
        {
            // play back
            TheChuck.Instance.RunCode( string.Format(
                @"SndBuf s => dac;
                ""{0}"" => s.read;
                s.length() => now;
                ",
                haveRecordedOnce ? myFilename : "special:dope"
            ) );
        }
        else
        {
            // finish recording and save
            MicrophoneController.StopRecording( myFilename );
            haveRecordedOnce = true;

            // reset color
            for( int i = 0; i < myShapes.Length; i++ )
            {
                myShapes[i].material.color = myShapesOriginalColors[i];
            }
        }
    }

    public void TouchpadAxis( Vector2 pos )
    {
        // don't care
    }

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // no settings to be copied over when cloned within-language
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // legacy, remember my name (cus you're gonna see me singing in the hall of fame!)
        return new string[] { myFilename };
    }

    public int[] SerializeIntParams( int version )
    {
        // store whether have recorded once
        return new int[] { haveRecordedOnce ? 1 : 0 };
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, float[] floatParams )
    {
        haveRecordedOnce = intParams[0] == 1;
        if( haveRecordedOnce )
        {
            // load my filename
            myFilename = stringParams[0];
        }
        else
        {
            // get a new one 
            myFilename = GetNextName();
        }
    }

    public void OnTrash()
    {
        // called by TrashController
        File.Delete( myFilename );
    }
}


