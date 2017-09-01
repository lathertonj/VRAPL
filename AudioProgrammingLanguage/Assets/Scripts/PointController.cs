using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointController : MonoBehaviour , IMoveMyself , IControllerInputAcceptor , IControllable
{

    public float pointHeight;
    public float rotationSpeed = 20;
    public float minConnectorSize = 0.1f;
    public float minSphereSize = 0.1f;
    private float currentHeightRange = 0.25f;
    private float currentRotation = 0;

    public Transform myPole, myKnob, myConnector, mySphere, myRod, myConnectorHolder, mySphereHolder;
    private Vector3 rodOrigScale;

    public Collider myPoleCollider, myKnobCollider, myConnectorCollider, mySphereCollider;
    public TrailRenderer myTrail;
    public MeshRenderer mySphereRenderer;
    
    // need to store initial lengths for the ones that can be elongated
    private Vector3 myPoleInitialPosition;
    private float myKnobInitialHeight, myConnectorInitialLength, 
        myConnectorInitialAngle, myConnectorInitialRadius, mySphereInitialSize;
    private int numPartsBeingMoved = 0;
    
    private List<ControlWorldObjectController> myHeightControllers;

    private string[] myAcceptableControls;

    // touchpad interface
    bool touchpadPressed = false;
    float currentHue;
    float currentSaturation;
    float currentColorValue;

	// Use this for initialization
	void Start () {
		rodOrigScale = myRod.localScale;
        myHeightControllers = new List<ControlWorldObjectController>();
        myAcceptableControls = new string[] { "height" };

        Color.RGBToHSV( mySphereRenderer.material.color, out currentHue, out currentSaturation, out currentColorValue );
	}

    void UpdateHue()
    {
        Color newColor = Color.HSVToRGB( currentHue, currentSaturation, currentColorValue );
        mySphereRenderer.material.color = newColor;
        myTrail.startColor = newColor;
    }
	
	// Update is called once per frame
	void Update () {

        // only move the sphere up and down and around if the rig isn't being moved
        if( numPartsBeingMoved == 0 )
        {
            // compute new height
            float newPointHeight = 0;
            foreach( ControlWorldObjectController controller in myHeightControllers )
            {
                // NormValue goes from 0 to 1
                // Make it go from -1 to 1 then multiply by height range
                newPointHeight += ( controller.NormValue() * 2 - 1 ) * currentHeightRange;
            }
            pointHeight = newPointHeight;

            // display height
		    mySphere.localPosition = new Vector3( 0, pointHeight, 0 );
            myRod.localPosition = new Vector3( 0, pointHeight / 2 - Mathf.Sign( pointHeight ) * rodOrigScale.z / 2, 0 );
            myRod.localScale = new Vector3( rodOrigScale.x, Mathf.Abs( pointHeight ), rodOrigScale.z );

            // do rotation on myconnectorholder
            currentRotation += Time.deltaTime * rotationSpeed;
        }

        // always set our angle
        Vector3 prevAngles = myConnectorHolder.localEulerAngles;
        prevAngles.y = currentRotation;
		myConnectorHolder.localEulerAngles = prevAngles;
	}

    public void StartMovement( Collider collider, Vector3 start )
    {
        if( collider == myPoleCollider )
        {
            myPoleInitialPosition = transform.position;
        }
        else if( collider == myKnobCollider )
        {
            myKnobInitialHeight = myKnobCollider.transform.position.y;
        }
        else if( collider == myConnectorCollider )
        {
            myConnectorInitialLength = myConnectorCollider.transform.localScale.x;
            myConnectorInitialAngle = currentRotation;
            Vector2 difference = new Vector2( start.x - transform.position.x, start.z - transform.position.z );
            myConnectorInitialRadius = difference.magnitude;
        }
        else if( collider == mySphereCollider )
        {
            mySphereInitialSize = mySphere.localScale.x;
        }
        else
        {
            // if we don't recognize the collider, it's probably because we were just generated
            // pretend it was myPoleCollider
            myPoleInitialPosition = transform.position;
        }
        numPartsBeingMoved++;
    }

    public void Move( Collider collider, Vector3 moveStart, Vector3 moveCurrent )
    {
        Vector3 moveDifference = moveCurrent - moveStart;
        if( collider == myPoleCollider )
        {
            // don't move in the y direction
            moveDifference.y = 0;
            // we are moving the entire thing
            transform.position = myPoleInitialPosition + moveDifference;
        }
        else if( collider == myKnobCollider )
        {
            // we are changing the height of the pole, the knob, and the connector holder
            float newHeight = myKnobInitialHeight + moveDifference.y;
            
            // pole
            Vector3 poleScale = myPole.localScale;
            poleScale.y = newHeight;
            myPole.localScale = poleScale;

            Vector3 polePosition = myPole.localPosition;
            polePosition.y = newHeight / 2;
            myPole.localPosition = polePosition;

            // knob
            Vector3 knobPosition = myKnob.localPosition;
            knobPosition.y = newHeight;
            myKnob.localPosition = knobPosition;

            // connector holder
            Vector3 connectorPosition = myConnectorHolder.localPosition;
            connectorPosition.y = newHeight;
            myConnectorHolder.localPosition = connectorPosition;
        }
        else if( collider == myConnectorCollider )
        {
            // we are changing the length of the connector and the position of the sphereholder
            /* This was for before the angle was also changed. 
             * It involved projecting vectors onto the vector of the connector.
            // don't allow movement in y direction to change length
            moveDifference.y = 0;
            // project moveDifference onto the vertical plane of the current angle
            Vector3 inDirectionOf = mySphere.position - myKnob.position;
            inDirectionOf.y = 0;
            Vector3 projectedDifference = Vector3.Project( moveDifference, inDirectionOf );
            // is it lengthening or shortening?
            float sign = 1;
            if( Mathf.Abs( Vector3.Angle( inDirectionOf, moveDifference ) ) >= 90 )
            {
                sign = -1;
            }
            float newLength = myConnectorInitialLength + sign * projectedDifference.magnitude;
            newLength = Mathf.Max( newLength, minConnectorSize );
            */
            Vector2 difference = new Vector2( moveCurrent.x - transform.position.x, moveCurrent.z - transform.position.z );
            float newLength = myConnectorInitialLength + difference.magnitude - myConnectorInitialRadius;

            // connector
            Vector3 connectorScale = myConnector.localScale;
            connectorScale.x = newLength;
            myConnector.localScale = connectorScale;

            Vector3 connectorPosition = myConnector.localPosition;
            connectorPosition.x = ( newLength - connectorScale.z ) / 2;
            myConnector.localPosition = connectorPosition;

            // sphereHolder
            Vector3 spherePosition = mySphereHolder.localPosition;
            spherePosition.x = newLength;
            mySphereHolder.localPosition = spherePosition;
            

            // we are also changing our angle
            Vector3 relationToSelf = moveCurrent - transform.position;
            currentRotation = - Mathf.Atan2( relationToSelf.z, relationToSelf.x ) * Mathf.Rad2Deg;

        }
        else if( collider == mySphereCollider )
        {
            // we are changing the size of the sphere and the max distance of the rod
            // * 2 to adjust for diameter / radius
            float newSize = mySphereInitialSize + moveDifference.y * 2;
            // not too small and also don't consume the entire rod
            newSize = Mathf.Clamp( newSize, minSphereSize, myConnector.localScale.x * 2 - 0.1f );

            // sphere size
            mySphere.localScale = new Vector3( newSize, newSize, newSize );

            // heightRange
            currentHeightRange = newSize * 1.5f;
        }
        else
        {
            // if we don't recognize the collider, it's probably because we were just generated
            // pretend it was myPoleCollider
            // don't move in the y direction
            moveDifference.y = 0;
            // we are moving the entire thing
            transform.position = myPoleInitialPosition + moveDifference;
        }
    }

    public void EndMovement( Collider collider )
    {
        numPartsBeingMoved--;
    }

    public void TouchpadDown()
    {
        touchpadPressed = true;
    }

    public void TouchpadUp()
    {
        touchpadPressed = false;
    }

    public void TouchpadAxis( Vector2 pos )
    {
        if( touchpadPressed )
        {
            currentHue += pos.y * 0.01f;
            while( currentHue < 0 )
            {
                currentHue += 1;
            }
            currentHue %= 1f;
            UpdateHue();
        }
    }

    public void TouchpadTransform( Transform t )
    {
        // don't care
    }

    public string[] AcceptableControls()
    {
        return myAcceptableControls;
    }

    public void StartControlling(string param, ControlWorldObjectController controller)
    {
        if( param == myAcceptableControls[0] )
        {
            myHeightControllers.Add( controller );
        }
    }

    public void StopControlling(string param, ControlWorldObjectController controller)
    {
        if( param == myAcceptableControls[0] )
        {
            myHeightControllers.Remove( controller );
        }
    }
}
