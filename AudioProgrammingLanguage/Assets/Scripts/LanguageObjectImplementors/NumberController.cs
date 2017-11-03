using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(NumberProducer))]
public class NumberController : MonoBehaviour , ILanguageObjectListener , IControllerInputAcceptor
{
    public GameObject myText;
    public GameObject myShape;

    private string myStorageClass;
    private string myExitEvent;
    private bool touchpadPressed = false;
    private float myNumber = 1.0f;
    private float myChangeSensitivity = 0.01f;
    private Vector3 startTransformPosition = Vector3.zero;
    private bool usingAxisRegion = true;

    private Color originalTextColor;
    private Color originalBodyColor;

    private ILanguageObjectListener myParent = null;
    private LanguageObject myLO;
    private ChuckSubInstance myChuck = null;

    // Use this for initialization
    void Awake()
    {
		UpdateMyNumber();
        originalTextColor = myText.GetComponent<TextMesh>().color;
        originalBodyColor = myShape.GetComponent<Renderer>().material.color;
        myLO = GetComponent<LanguageObject>();
	}
	
	// Update is called once per frame
	void Update () {
		//myChangeSensitivity = 0.01f + 3 * ( GetComponent<MovableController>().myScale - 1 );
	}

    public void SetColors( Color body, Color text )
    {
        myShape.GetComponent<Renderer>().material.color = body;
        myText.GetComponent<TextMesh>().color = text;
    }

    private void SwitchColors()
    {
        Color tempColor = myText.GetComponent<TextMesh>().color;
        myText.GetComponent<TextMesh>().color = myShape.GetComponent<Renderer>().material.color;
        myShape.GetComponent<Renderer>().material.color = tempColor;
    }

    
    public bool AcceptableChild( LanguageObject other )
    {
        return false;
    }

    public void NewChild( LanguageObject child )
    {
        // don't care
    }

    public void NewParent(LanguageObject parent)
    {
        ILanguageObjectListener p = (ILanguageObjectListener) parent.GetComponent(typeof(ILanguageObjectListener));
        if( p != null )
        {
            myParent = p;
            SwitchColors();
        }
    }

    public void ParentDisconnected(LanguageObject parent)
    {
        if( myParent != null )
        {
            // SwitchColors();
            SetColors( originalBodyColor, originalTextColor );
            myParent = null;
        }
    }

    public void ChildDisconnected(LanguageObject child)
    {
        // don't care
    }

    public void GotChuck(ChuckSubInstance chuck)
    {
        myChuck = chuck;
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        chuck.RunCode(string.Format(@"
            external Event {1};
            public class {0}
            {{
                static Step @ myStep;
            }}
            Step s @=> {0}.myStep;
            {2} => {0}.myStep.next;

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent, myNumber.ToString("0.00") ));

        if( myParent != null )
        {
            chuck.RunCode(string.Format("{0} => {1};", OutputConnection(), myParent.InputConnection( myLO ) ) );
        }
    }

    public void LosingChuck(ChuckSubInstance chuck)
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
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format("{0}.myStep", myStorageClass);
    }

    public void TouchpadDown()
    {
        touchpadPressed = true;
    }

    public void TouchpadUp()
    {
        touchpadPressed = false;
        startTransformPosition = Vector3.zero;
    }

    public void TouchpadAxis( Vector2 pos )
    {
        /*if( touchpadPressed && usingAxisRegion )
        {
            IncrementMyNumber( pos.y * myChangeSensitivity );
        }*/
    }

    public void TouchpadTransform( Transform t )
    {
        if( startTransformPosition == Vector3.zero )
        {
            startTransformPosition = t.position;
        }
        else
        {
            float distanceChange = t.position.y - startTransformPosition.y;
            float distanceDirection = Mathf.Sign( distanceChange );
            float noSensitivityRegion = 0.03f;
            float midSensitivityRegion = 0.05f;
            // subtract out the no sensitivity region
            float distanceMagnitude = Mathf.Max( Mathf.Abs( distanceChange ) - midSensitivityRegion, 0 );

            float baseRate = myChangeSensitivity * distanceDirection * 0.05f;

            if( Mathf.Abs( distanceChange ) < noSensitivityRegion )
            {
                // we are inside the safe zone. do not alter number, and allow axis to alter number.
                usingAxisRegion = true;
            }
            else if( Mathf.Abs( distanceChange ) < midSensitivityRegion )
            {
                // change number at a constant rate
                // TODO: bug where if the existing number already has a big magnitude, then adding this to it
                // does nothing. I think this is to do with limitations of floats.
                // Should the display update (i.e. keep track of integer and decimal separately)
                // even when that number would be inaccurate?
                usingAxisRegion = false;
                IncrementMyNumber( baseRate );
            }
            else
            {
                // we are outside the safe zone. alter number by transform distance, not axis.
                usingAxisRegion = false;
                IncrementMyNumber( baseRate + myChangeSensitivity * distanceDirection * Mathf.Pow( distanceMagnitude, 6 ) * 10000 );
            }
        }
    }

    void IncrementMyNumber( float inc )
    {
        myNumber += inc;
        UpdateMyNumber();
    }

    void UpdateMyNumber()
    {
        // round number in display
        myText.GetComponent<TextMesh>().text = myNumber.ToString("0.00");
        if( myChuck != null )
        {
            // round number in chuck as well
            myChuck.RunCode(string.Format(@"
                {0} => {1}.next;
                ",
                myNumber.ToString("0.00"), OutputConnection() 
            ));
        }
    }

    public float GetValue()
    {
        return myNumber;
    }
    
    public string VisibleName()
    {
        return myText.GetComponent<TextMesh>().text;
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        NumberController other = original.GetComponent< NumberController >();

        myNumber = other.myNumber;
        UpdateMyNumber();
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
        // store my number
        return new float[] { myNumber };
    }

    public object[] SerializeObjectParams( int version )
    {
        // no object params
        return LanguageObject.noObjectParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams )
    {
        // load my number
        myNumber = floatParams[0];
        UpdateMyNumber();
    }
}
