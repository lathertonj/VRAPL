using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(WorldObject))]
public class ControllableWorldObject : MonoBehaviour , IControllable
{
    public string[] myAcceptableControls = new string[] { "force: x", "force: y", "force: z" };

    private Dictionary< string, List< ControlWorldObjectController > > myControllers;
    private Dictionary< string, float > currentControllerValues;

    private Rigidbody rb;

    // Use this for initialization
    void Start () {
        myControllers = new Dictionary< string, List< ControlWorldObjectController > >();
        currentControllerValues = new Dictionary< string, float >();
		foreach( string control in myAcceptableControls )
        {
            myControllers[control] = new List< ControlWorldObjectController >();
            currentControllerValues[control] = 0;
        }

        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        // Update current values from controllers
		foreach( string control in myAcceptableControls )
        {
            currentControllerValues[control] = 0;
            foreach( ControlWorldObjectController controller in myControllers[control] )
            {
                currentControllerValues[control] += controller.CurrentValue();
            }
        }

        // Do something with each value -- visual updates
	}

    private void FixedUpdate()
    {
        // Do something with each value -- physics
        rb.AddForce( WorldSize.currentWorldSize * new Vector3( 
            currentControllerValues["force: x"],
            currentControllerValues["force: y"],
            currentControllerValues["force: z"]
        ) );
    }

    public string[] AcceptableControls()
    {
        return myAcceptableControls;
    }

    public void StartControlling( string param, ControlWorldObjectController controller )
    {
        if( myControllers.ContainsKey( param ) )
        {
            myControllers[param].Add( controller );
        }
    }

    public void StopControlling( string param, ControlWorldObjectController controller )
    {
        if( myControllers.ContainsKey( param ) )
        {
            myControllers[param].Remove( controller );
        }
    }

    // Use this class for objects that should be able to do everything and have a rigidbody
}


public interface IControllable
{
    string[] AcceptableControls();
    void StartControlling( string param, ControlWorldObjectController controller );
    void StopControlling( string param, ControlWorldObjectController controller );
}
