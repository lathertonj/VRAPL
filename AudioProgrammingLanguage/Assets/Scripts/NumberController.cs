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

    private Color originalTextColor;
    private Color originalBodyColor;

    private ILanguageObjectListener myParent = null;

    // Use this for initialization
    void Start () {
		UpdateMyNumber();
        originalTextColor = myText.GetComponent<TextMesh>().color;
        originalBodyColor = myShape.GetComponent<Renderer>().material.color;
	}
	
	// Update is called once per frame
	void Update () {
		myChangeSensitivity = 0.01f + 3 * ( GetComponent<MovableController>().myScale - 1 );
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

    
    public bool AcceptableChild(LanguageObject other, Collider mine)
    {
        return false;
    }

    public void NewChild(LanguageObject child, Collider mine)
    {
        // ???
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

    public void GotChuck(ChuckInstance chuck)
    {
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
            chuck.RunCode(string.Format("{0} => {1};", OutputConnection(), myParent.InputConnection() ) );
        }
    }

    public void LosingChuck(ChuckInstance chuck)
    {
        if( myParent != null )
        {
            chuck.RunCode(string.Format("{0} =< {1};", OutputConnection(), myParent.InputConnection() ) );
        }

        chuck.BroadcastEvent( myExitEvent );
    }

    public string InputConnection()
    {
        return string.Format("{0}.myStep", myStorageClass);
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public ChuckInstance GetChuck()
    {
        return GetComponent<LanguageObject>().GetChuck();
    }

    public void TouchpadDown()
    {
        touchpadPressed = true;
    }

    public void TouchpadUp()
    {
        touchpadPressed = false;
    }

    public void TouchpadAxis(Vector2 pos)
    {
        if( touchpadPressed )
        {
            myNumber += pos.y * myChangeSensitivity;
            UpdateMyNumber();
        }
    }

    void UpdateMyNumber()
    {
        // TODO: should the number itself be rounded?
        // myNumber = (float) Math.Round( myNumber, 2, MidpointRounding.AwayFromZero );
        myText.GetComponent<TextMesh>().text = myNumber.ToString("0.00");
        if( GetChuck() != null )
        {
            GetChuck().RunCode(string.Format(@"
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
}
