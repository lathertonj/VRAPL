using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovableController))]
[RequireComponent(typeof(EventNotifyController))]
public class WorldObject : MonoBehaviour 
{

    public Transform myPrefab;

    private Dictionary<DacController, bool> myPrograms;
    private Dictionary<ControlWorldObjectController, bool> myControllers;

    private Dictionary<LanguageObject, int> currentCollisionCounts;
    private Dictionary<LanguageObject, int> enteringDebounceObjects;
    private Dictionary<LanguageObject, int> exitingDebounceObjects;
    private int debounceFramesToWait = 1;

    private IControllable myControllable;

    private int myWorldObjectID;

	// Use this for initialization
	void Start () {
        myPrograms = new Dictionary<DacController, bool>();
        myControllers = new Dictionary<ControlWorldObjectController, bool>();

        currentCollisionCounts = new Dictionary<LanguageObject, int>();
		enteringDebounceObjects = new Dictionary<LanguageObject, int>();
        exitingDebounceObjects = new Dictionary<LanguageObject, int>();

        myControllable = (IControllable) GetComponent(typeof(IControllable));
	}
	
	// Update is called once per frame
	void Update () {
        // sometimes an enter-exit-enter fires in quick succession.
        // debouncing makes each one wait a frame to see if the other fires to negate it
        // this way, an enter-exit-enter or an exit-enter-exit will just be sent as 
        // an enter or an exit to the listening object.
        CheckDebounce( enteringDebounceObjects, LanguageObjectEntered );
        CheckDebounce( exitingDebounceObjects, LanguageObjectExited );
	}

    void CheckDebounce( Dictionary< LanguageObject, int > debounce, 
                        Action< LanguageObject > callback )
    {
        List<LanguageObject> keys = new List<LanguageObject>( debounce.Keys );
        foreach( LanguageObject key in keys )
        {
		    // subtract 1 from all debounce actions;
            debounce[key]--;
            // if < 0, actually run it
            if( debounce[key] < 0 )
            {
                callback( key );
                debounce.Remove( key );
            }
        }
    }

    void OnTriggerEnter( Collider other )
    {
        LanguageObject lo = LanguageObject.SearchForLanguageObject( other );
        if( !lo )
        {
            // not colliding with a language object
            return;
        }

        if( !currentCollisionCounts.ContainsKey( lo ) )
        {
            currentCollisionCounts[lo] = 0;
        }
        currentCollisionCounts[lo]++;
        if( currentCollisionCounts[lo] == 1 )
        {
            // first colliding
            // if we're waiting to execute an exit for this object, cancel the exit
            if( exitingDebounceObjects.ContainsKey( lo ) )
            {
                exitingDebounceObjects.Remove( lo );
            }
            // else, schedule the entrance
            else
            {
                enteringDebounceObjects[lo] = debounceFramesToWait;
            }
        }
    }

    void OnTriggerExit( Collider other )
    {
        LanguageObject lo = LanguageObject.SearchForLanguageObject( other );
        if( !lo )
        {
            // didn't find a language object. do nothing
            return;
        }

        currentCollisionCounts[lo]--;
        if( currentCollisionCounts[lo] == 0 )
        {
            // finally exiting.
            // if we're waiting to execute an entrance for this object, cancel the entrance
            if( enteringDebounceObjects.ContainsKey( lo ) )
            {
                enteringDebounceObjects.Remove( lo );
            }
            // else, schedule the entrance
            else
            {
                exitingDebounceObjects[lo] = debounceFramesToWait;
            }
        }
        else if (currentCollisionCounts[lo] < 0 )
        {
            Debug.LogError("Problem! " + gameObject.name + " has less than 0 collisions with " + lo.gameObject.name );
        }
    }

    void LanguageObjectEntered( LanguageObject other )
    {
        if( !other.GetComponent<MovableController>().amBeingMoved )
        {
            // other is not being moved. we can't drop world objects onto language objects. do nothing.
            return;
        }

        DacController maybeDac = other.GetComponent<DacController>();
        if( maybeDac != null && !myPrograms.ContainsKey( maybeDac ) )
        {
            // I have found a new program!
            myPrograms[maybeDac] = true;
            // make it my child when it is done being moved
            other.GetComponent<MovableController>().parentAfterMovement = transform;
        }

        ControlWorldObjectController maybeController = other.GetComponent<ControlWorldObjectController>();
        if( maybeController != null && !myControllers.ContainsKey( maybeController ) )
        {
            // I have found a new controller!
            myControllers[maybeController] = true;
            // make it my child when it is done being moved
            other.GetComponent<MovableController>().parentAfterMovement = transform;
            // track it on myself
            if( myControllable != null )
            {
                maybeController.ParentConnected( myControllable );
            }
        }
    }

    void LanguageObjectExited( LanguageObject other )
    {
        if( !other.GetComponent<MovableController>().amBeingMoved )
        {
            // it was colliding but it didn't end up being a part of us
            return;
        }

        DacController maybeDac = other.GetComponent<DacController>();
        if( maybeDac != null && myPrograms.ContainsKey( maybeDac ) )
        {
            myPrograms.Remove( maybeDac );
            other.GetComponent<MovableController>().parentAfterMovement = null;
        }

        ControlWorldObjectController maybeController = other.GetComponent<ControlWorldObjectController>();
        if( maybeController != null && myControllers.ContainsKey( maybeController ) )
        {
            myControllers.Remove( maybeController );
            other.GetComponent<MovableController>().parentAfterMovement = null;
            if( myControllable != null )
            {
                maybeController.ParentDisconnected( myControllable );
            }
        }
    }

    public GameObject MakeLanguageObjectDataReporter()
    {
        // make a copy of the prefab
        Transform newLanguageObject = Instantiate( myPrefab, transform.position, transform.rotation );
        // make it smaller
        newLanguageObject.localScale = newLanguageObject.localScale * 0.2f;
        // this does not add LanguageObject since LanguageObject needs to have access to DataReporter on its init
        newLanguageObject.gameObject.AddComponent<DataReporter>();
        // this does
        newLanguageObject.gameObject.AddComponent<LanguageObject>();
        // prevent instantiations from the new object
        Destroy( newLanguageObject.GetComponent<WorldObject>() );
        // hook in the aspects of me that DataReporter will access
        newLanguageObject.GetComponent<DataReporter>().myRigidbody = GetComponent<Rigidbody>();
        // when I get an event, tell newLanguageObject
        GetComponent<EventNotifyController>().AddListener( newLanguageObject.GetComponent<EventNotifyController>() );

        return newLanguageObject.gameObject;
    }


    public void OnCollisionEnter( Collision collision )
    {
        // trigger an even with intensity == collision velocity magnitude, normalized a bit
        GetComponent<EventNotifyController>().TriggerEvent( Mathf.Clamp01( collision.relativeVelocity.magnitude / 10 ) );
    }

}
