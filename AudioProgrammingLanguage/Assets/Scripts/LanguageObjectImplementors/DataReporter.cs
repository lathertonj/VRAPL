using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(EventNotifyController))]
public class DataReporter : MonoBehaviour , ILanguageObjectListener , IDataSource , IControllerInputAcceptor , IEventNotifyResponder
{
    public static Transform textPrefab = null;
    public Rigidbody myRigidbody = null;

    private TextMesh myText;

    private float myMax, myMin, myCurrent;
    private float lastCollisionIntensity = 0;

    private string[] modes;
    private int currentModeIndex;
    private string currentMode;

    private void Awake()
    {
        CreateMyText();
    }

    // TODO: data shaper block

    // Use this for initialization
    void Start () {
        // TODO: modes for events!
        // TODO: what to do for things that don't have a min / max value???
		modes = new string[] { "movement: x", "movement: y", "movement: z", "movement: any",
                               "rotation: x", "rotation: y", "rotation: z", "rotation: any",
                               "size", "collision intensity" };
        currentModeIndex = 0;
        currentMode = modes[currentModeIndex];
        myText.text = currentMode;

        UpdateMinAndMax();
	}

    void CreateMyText()
    {
        // create new text
        Transform newText = Instantiate( BasicTextHolder.basicTextPrefab, transform.position, transform.rotation );
        // assign to me
        newText.parent = transform;
        // store
        myText = newText.GetComponent<TextMesh>();
        // set color 
        myText.color = Color.white;
    }
	
	void Update () {
        if( myRigidbody != null  )
        {
            switch( currentMode )
            {
                case "movement: x":
                    myCurrent = myRigidbody.velocity.x;
                    break;
                case "movement: y":
                    myCurrent = myRigidbody.velocity.y;
                    break;
                case "movement: z":
                    myCurrent = myRigidbody.velocity.z;
                    break;
                case "movement: any":
                    myCurrent = myRigidbody.velocity.magnitude;
                    break;
                case "rotation: x":
                    myCurrent = myRigidbody.angularVelocity.x;
                    break;
                case "rotation: y":
                    myCurrent = myRigidbody.angularVelocity.y;
                    break;
                case "rotation: z":
                    myCurrent = myRigidbody.angularVelocity.z;
                    break;
                case "rotation: any":
                    myCurrent = myRigidbody.angularVelocity.magnitude;
                    break;
                case "size":
                    myCurrent = myRigidbody.GetComponent<MovableController>().GetScale();
                    break;
                case "collision intensity":
                    myCurrent = lastCollisionIntensity;
                    break;
            }
        }
	}

    void UpdateMinAndMax()
    {
        switch( currentMode )
        {
            case "movement: x":
            case "movement: y":
            case "movement: z":
                myMin = -4.5f;
                myMax = 4.5f;
                break;
            case "movement: any":
                myMin = 0f;
                myMax = 4.5f;
                break;
            case "rotation: x":
            case "rotation: y":
            case "rotation: z":
                myMin = -30;
                myMax = 30;
                break;
            case "rotation: any":
                myMin = 0;
                myMax = 30;
                break;
            case "size":
                myMin = myRigidbody.GetComponent<MovableController>().myMinScale;
                // how to decide... maybe 10 * min is fine?
                myMax = 5 * myMin;
                break;
            case "collision intensity":
                myMin = 0;
                myMax = 1;
                break;
        }
    }


    public void NewParent( LanguageObject parent )
    {
        myText.color = Color.black;
    }


    public void ParentDisconnected( LanguageObject parent )
    {
        myText.color = Color.white;
    }
    
    public float CurrentValue()
    {
        return myCurrent;
    }

    public float MaxValue()
    {
        return myMax;
    }

    public float MinValue()
    {
        return myMin;
    }

    public float NormValue()
    {
        return Mathf.Clamp01( ( myCurrent - myMin ) / ( myMax - myMin ) );
    }

    public bool AcceptableChild( LanguageObject other )
    {
        return false;
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
        // don't care
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        // don't care
    }

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string InputConnection()
    {
        // don't have one
        return "";
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public void TouchpadDown()
    {
        // change mode
        currentModeIndex++;
        currentModeIndex %= modes.Length;
        currentMode = modes[currentModeIndex];
        myText.text = currentMode;
        UpdateMinAndMax();
    }

    public void TouchpadUp()
    {
        // don't care
    }

    public void TouchpadAxis( Vector2 pos )
    {
        // don't care
    }

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public void RespondToEvent( float intensity )
    {
        lastCollisionIntensity = intensity;
    }

    public string VisibleName()
    {
        return myText.text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        DataReporter other = original.GetComponent<DataReporter>();
        // simulate touchpad presses until we match the mode of the original
        while( currentModeIndex != other.currentModeIndex )
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
        // store currentModeIndex
        return new int[] { currentModeIndex };
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
        // simulate touchpad presses until we match the mode of the original
        while( currentModeIndex != intParams[0] )
        {
            TouchpadDown();
        }
    }
}
