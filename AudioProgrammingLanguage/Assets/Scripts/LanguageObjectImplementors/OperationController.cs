using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NumberProducer))]
[RequireComponent(typeof(LanguageObject))]
public class OperationController : MonoBehaviour , ILanguageObjectListener , IControllerInputAcceptor
{

    public TextMesh myText;
    public MeshRenderer myShape;

    private int myCurrentIndex;
    private string myCurrentOp;
    private string[] myOps;

    private string myStorageClass;
    private string myExitEvent;
    private string myChangeOpEvent;
    private ILanguageObjectListener myParent = null;
    private LanguageObject myLO = null;
    private LanguageObject myLeftArg = null;
    private LanguageObject myRightArg = null;
    private ChuckSubInstance myChuck = null;

	// Use this for initialization
	public void InitLanguageObject( ChuckSubInstance chuck ) 
    {
        // init object
		myOps = new string[] { ">", "<", ">=", "<=" };
        myCurrentIndex = 0;
        myCurrentOp = myOps[myCurrentIndex];
        myText.text = myCurrentOp;
        myLO = GetComponent<LanguageObject>();

        // init chuck
        myChuck = chuck;
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();
        myChangeOpEvent = chuck.GetUniqueVariableName();

        chuck.RunCode(string.Format(@"
            external Event {1};
            external Event {2};
            public class {0}
            {{
                static Step @ myOutput;
                static Gain @ myLeftInput;
                static Gain @ myRightInput;
            }}
            Gain g1 @=> {0}.myLeftInput;
            {0}.myLeftInput => blackhole;
            Gain g2 @=> {0}.myRightInput;
            {0}.myRightInput => blackhole;
            Step s @=> {0}.myOutput;

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent, myChangeOpEvent ));

        UpdateMyOp();
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public void UpdateMyOp()
    {
        // end the last one by signaling the event at the beginning
        myChuck.RunCode( string.Format( @"
            external Event {1};
            {1}.broadcast();

            fun void DoTheOp()
            {{
                while( true )
                {{
                    ( {0}.myLeftInput.last() {2} {0}.myRightInput.last() ) => {0}.myOutput.next;
                    0.5::ms => now;
                }}
            }}
            
            spork ~ DoTheOp();

            {1} => now;
        ", myStorageClass, myChangeOpEvent, myCurrentOp ) );
    }
	
	void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShape.material.color;
        myShape.material.color = temp;
    }

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        myParent = parentListener;
        SwitchColors();
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        if( parentListener == myParent )
        {
            SwitchColors();
            myParent = null;
        }
    }

    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        // is it incorrect type?
        if( other.GetComponent<SoundProducer>() == null &&
            other.GetComponent<NumberProducer>() == null )
        {
            // can't make sounds or numbers. not acceptable.
            return false;
        }
        
        // do I have a free space there?
        if( WouldBeLeft( other ) )
        {
            // do I have space on the left side?
            return myLeftArg == null;
        }
        else
        {
            // do I have space on the right side?
            return myRightArg == null;
        }
    }


    private bool WouldBeLeft( LanguageObject potentialChild )
    {
        // is it left or is it right? temporarily put it in me and check localPosition.x
        Transform oldParent = potentialChild.transform.parent;
        potentialChild.transform.parent = transform;
        float localXPos = potentialChild.transform.localPosition.x;
        potentialChild.transform.parent = oldParent;

        return localXPos <= 0;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( WouldBeLeft( child ) )
        {
            myLeftArg = child;
        }
        else
        {
            myRightArg = child;
        }
        LanguageObject.HookTogetherLanguageObjects( myChuck, child, myLO );
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        LanguageObject.UnhookLanguageObjects( myChuck, child, myLO );
        // check and update internal storage
        if( child == myLeftArg )
        {
            myLeftArg = null;
        }
        else if( child == myRightArg )
        {
            myRightArg = null;
        }
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        if( whoAsking == myLeftArg )
        {
            return string.Format( "{0}.myLeftInput", myStorageClass );
        }
        else if( whoAsking == myRightArg )
        {
            return string.Format( "{0}.myRightInput", myStorageClass );
        }
        else
        {
            return "blackhole";
        }
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myOutput", myStorageClass );
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string VisibleName()
    {
        return myText.text;
    }

    public void TouchpadDown()
    {
        myCurrentIndex++;
        myCurrentIndex %= myOps.Length;
        myCurrentOp = myOps[myCurrentIndex];
        myText.text = myCurrentOp;
        UpdateMyOp();
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
        OperationController other = original.GetComponent<OperationController>();

        // simulate touchpad presses until state matches
        while( myCurrentIndex != other.myCurrentIndex )
        {
            TouchpadDown();
        }
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // no string params
        return LanguageObject.noStringParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // current index
        return new int[] { myCurrentIndex };
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
        // simulate touchpad presses until state matches
        while( myCurrentIndex != intParams[0] )
        {
            TouchpadDown();
        }
    }
}
