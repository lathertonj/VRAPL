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
    private ChuckInstance myChuck = null;

	// Use this for initialization
	void Awake() 
    {
		myOps = new string[] { ">", "<", ">=", "<=" };
        myCurrentIndex = 0;
        myCurrentOp = myOps[myCurrentIndex];
        myText.text = myCurrentOp;
        myLO = GetComponent<LanguageObject>();
	}
	
	void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShape.material.color;
        myShape.material.color = temp;
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

    public bool AcceptableChild( LanguageObject other )
    {
        // TODO: is it correct type and I have a free space there?
        return false;
    }

    public void NewParent( LanguageObject parent )
    {
        ILanguageObjectListener lo = (ILanguageObjectListener) parent.GetComponent( typeof( ILanguageObjectListener ) );
        if( lo != null )
        {
            myParent = lo;
            SwitchColors();
        }
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        ILanguageObjectListener lo = (ILanguageObjectListener) parent.GetComponent( typeof( ILanguageObjectListener ) );
        if( lo == myParent )
        {
            SwitchColors();
            myParent = null;
        }
    }

    public void NewChild( LanguageObject child )
    {
        // TODO: is it left or is it right?
    }

    public void ChildDisconnected(LanguageObject child)
    {
        // TODO: is it left or is it right?
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        // TODO
        return "";
    }

    public string OutputConnection()
    {
        // TODO
        return "";
    }

    public void GotChuck(ChuckInstance chuck)
    {
        myChuck = chuck;
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();
        myChangeOpEvent = chuck.GetUniqueVariableName();

        chuck.RunCode(string.Format(@"
            external Event {1};
            external Event {2};
            public class {0}
            {{
                static Gain @ myGain;
            }}
            Gain g @=> {0}.myGain;
            0.001 => {0}.myGain.gain;

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent, myChangeOpEvent ));

        UpdateMyOp();

        if( myParent != null )
        {
            chuck.RunCode(string.Format("{0} => {1};", OutputConnection(), myParent.InputConnection( myLO ) ) );
        }
    }

    public void LosingChuck(ChuckInstance chuck)
    {
        if( myParent != null )
        {
            chuck.RunCode(string.Format("{0} =< {1};", OutputConnection(), myParent.InputConnection( myLO ) ) );
        }

        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }

    public void UpdateMyOp()
    {

    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string VisibleName()
    {
        return myText.text;
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
