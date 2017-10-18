using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDataReporter : MonoBehaviour , ILanguageObjectListener , IControllerInputAcceptor , IDataSource
{

    public TextMesh myText;
    public MeshRenderer myShape;

    private string[] modes;
    private int currentModeIndex;
    private string currentMode;

    private float myMin, myMax, myCurrent;
    private Color myOriginalColor;
    private Color myOriginalTextColor;
    private Color myMinColor;
    private Color myMaxColor;
    private bool useChangingColor = false;

    public SteamVR_Controller.Device myController = null;
    public Transform myControllerPosition = null;

    // Use this for initialization
    void Awake()
    {
		modes = new string[] { "position: x", "position: y", "position: z",
                               "movement: x", "movement: y", "movement: z", "movement: any",
                               "rotation: x", "rotation: y", "rotation: z", "rotation: any" };
        currentModeIndex = 0;
        currentMode = modes[currentModeIndex];

        myText.text = currentMode;
        myOriginalColor = myShape.material.color;
        myOriginalTextColor = myText.color;
        myMinColor = Color.black;
        myMaxColor = Color.white;
        UpdateMinAndMax();
	}
	
	// Update is called once per frame
	void Update() 
    {
        if( myController != null && myControllerPosition != null )
        {
            // if( myController.angularVelocity.magnitude > 2.0f )
            //Debug.Log("y position: " + myController.angularVelocity.magnitude.ToString("0.000"));
            switch( currentMode )
            {
                case "position: x":
                    myCurrent = myControllerPosition.localPosition.x;
                    break;
                case "position: y":
                    myCurrent = myControllerPosition.localPosition.y;
                    break;
                case "position: z":
                    myCurrent = myControllerPosition.localPosition.z;
                    break;
                case "movement: x":
                    myCurrent = myController.velocity.x;
                    break;
                case "movement: y":
                    myCurrent = myController.velocity.y;
                    break;
                case "movement: z":
                    myCurrent = myController.velocity.z;
                    break;
                case "movement: any":
                    myCurrent = myController.velocity.magnitude;
                    break;
                case "rotation: x":
                    myCurrent = myController.angularVelocity.x;
                    break;
                case "rotation: y":
                    myCurrent = myController.angularVelocity.y;
                    break;
                case "rotation: z":
                    myCurrent = myController.angularVelocity.z;
                    break;
                case "rotation: any":
                    myCurrent = myController.angularVelocity.magnitude;
                    break;
            }

            // update color
            if( useChangingColor )
            {
                myShape.material.color = myMinColor + NormValue() * ( myMaxColor - myMinColor );
            }
        }
	}

    void UpdateMinAndMax()
    {
        switch( currentMode )
        {
            // TODO: get x, y from playable area
            case "position: x":
                myMin = -1.3f;
                myMax = 1.3f;
                break;
            case "position: y":
                myMin = 0;
                myMax = 1.8f;
                break;
            case "position: z":
                myMin = -0.75f;
                myMax = 0.75f;
                break;
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
        }
    }

    public bool AcceptableChild( LanguageObject other )
    {
        // no children for me!
        return false;
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

    public void NewChild( LanguageObject child )
    {
        // don't care
    }

    public void NewParent(LanguageObject parent)
    {
        // change color -- set in Update
        useChangingColor = true;
        // use white text to see against changing color
        myText.color = Color.white;
    }

    public void ParentDisconnected(LanguageObject parent)
    {
        // change color back
        useChangingColor = false;
        myShape.material.color = myOriginalColor;
        myText.color = myOriginalTextColor;
    }

    public void TouchpadAxis(Vector2 pos)
    {
        // don't care
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

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public float CurrentValue()
    {
        return myCurrent;
    }

    public float MinValue()
    {
        return myMin;
    }

    public float MaxValue()
    {
        return myMax;
    }

    public float NormValue()
    {
        return Mathf.Clamp01( ( myCurrent - myMin ) / ( myMax - myMin ) );
    }

    public void SetColors( Color min, Color max )
    {
        myMinColor = min;
        myMaxColor = max;
    }

    public string VisibleName()
    {
        return myText.text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        ControllerDataReporter other = original.GetComponent<ControllerDataReporter>();

        // simulate touchpad presses until it matches
        while( currentModeIndex != other.currentModeIndex )
        {
            TouchpadDown();
        }

        myController = other.myController;
        myControllerPosition = other.myControllerPosition;
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // store modes?
        return modes;
    }

    public int[] SerializeIntParams( int version )
    {
        // store my current index
        return new int[] { currentModeIndex };
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, float[] floatParams )
    {
        // get modes
        modes = stringParams;

        // simulate touchpad presses until it matches
        while( currentModeIndex != intParams[0] )
        {
            TouchpadDown();
        }
    }
}
