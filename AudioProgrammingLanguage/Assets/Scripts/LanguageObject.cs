using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[RequireComponent(typeof(MovableController))]
[RequireComponent(typeof(RendererController))]
[RequireComponent(typeof(Rigidbody))]
public class LanguageObject : MonoBehaviour {

    public ArrayList myChildren;
    public LanguageObject myParent = null;

    private Dictionary<LanguageObject, int> currentCollisionCounts;
    private Dictionary<LanguageObject, int> enteringDebounceObjects;
    private Dictionary<LanguageObject, int> exitingDebounceObjects;
    private Dictionary<LanguageObject, Collider> enteringDebounceColliders;
    private Dictionary<LanguageObject, Collider> exitingDebounceColliders;
    private int debounceFramesToWait = 1;

    private void Awake()
    {
        // recursively make sure all my colliders are trigger colliders and that my rigidbodies do not use gravity, etc
        EnsureTriggerBehavior( transform );
		myChildren = new ArrayList();
        currentCollisionCounts = new Dictionary<LanguageObject, int>();
        enteringDebounceObjects = new Dictionary<LanguageObject, int>();
        exitingDebounceObjects = new Dictionary<LanguageObject, int>();
        enteringDebounceColliders = new Dictionary<LanguageObject, Collider>();
        exitingDebounceColliders = new Dictionary<LanguageObject, Collider>();        
    }

    // Use this for initialization
    void Start () {
	}

    void EnsureTriggerBehavior( Transform self )
    {
        // is trigger
        Collider c = self.GetComponent<Collider>();
        if( c != null )
        {
            c.isTrigger = true;
        }
        // does not use gravity, is kinematic
        Rigidbody rb = self.GetComponent<Rigidbody>();
        if( rb != null )
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        foreach( Transform child in self.transform )
        {
            EnsureTriggerBehavior( child );
        }
    }

    // Update is called once per frame
    void Update () {
        // sometimes an enter-exit-enter fires in quick succession.
        // debouncing makes each one wait a frame to see if the other fires to negate it
        // this way, an enter-exit-enter or an exit-enter-exit will just be sent as 
        // an enter or an exit to the listening object.
        CheckDebounce( enteringDebounceObjects, enteringDebounceColliders, LanguageObjectFirstEntered );
        CheckDebounce( exitingDebounceObjects, exitingDebounceColliders, LanguageObjectLastExited );
	}

    void CheckDebounce( Dictionary< LanguageObject, int > debounce, 
                        Dictionary< LanguageObject, Collider > colliders,
                        Action< LanguageObject, Collider > callback )
    {
        List<LanguageObject> keys = new List<LanguageObject>( debounce.Keys );
        foreach( LanguageObject key in keys )
        {
		    // subtract 1 from all debounce actions;
            debounce[key]--;
            // if < 0, actually run it
            if( debounce[key] < 0 )
            {
                callback( key, colliders[key] );
                debounce.Remove( key );
            }
        }
    }

    void OnTriggerEnter( Collider other )
    {
        LanguageObject lo = SearchForLanguageObject( other );
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
                exitingDebounceColliders.Remove( lo );
            }
            // else, schedule the entrance
            else
            {
                enteringDebounceObjects[lo] = debounceFramesToWait;
                enteringDebounceColliders[lo] = other;
            }
        }
    }

    void OnTriggerExit( Collider other )
    {
        LanguageObject lo = SearchForLanguageObject( other );
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
                enteringDebounceColliders.Remove( lo );
            }
            // else, schedule the entrance
            else
            {
                exitingDebounceObjects[lo] = debounceFramesToWait;
                exitingDebounceColliders[lo] = other;
            }
        }
        else if (currentCollisionCounts[lo] < 0 )
        {
            Debug.LogError("Problem! " + gameObject.name + " has less than 0 collisions with " + lo.gameObject.name );
        }
    }

    void LanguageObjectFirstEntered( LanguageObject entering, Collider collisionWith )
    {
        // if language object wasn't ourself (colliding with component parts)
        if( entering != GetComponent<LanguageObject>() )
        {
            if( myParent )
            {
                // is this the right thing to do?
                // already have a parent, do nothing
                return;
            }

            if( !GetComponent<MovableController>().amBeingMoved )
            {
                // is this the right thing to do?
                // not being moved, do nothing
                return;
            }

            if( entering.GetComponent<MovableController>().amBeingMoved )
            {
                // other one is being moved too -- do nothing
                return;
            }

            ChuckInstance myChuck = GetChuck();
            ChuckInstance theirChuck = entering.GetChuck();

            if( ! ( ( (ILanguageObjectListener) entering.GetComponent( typeof(ILanguageObjectListener) ) )
                .AcceptableChild( GetComponent<LanguageObject>(), collisionWith ) ) )
            {
                // "entering" will not consider me as a child

                // will I consider "entering" as a child if they don't already have a parent?
                // TODO: in this region, should "collisionWith" be passed as null? because the relationship
                // is flipped so I don't know what the actual collision on the parent was?
                if( ( ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) )
                .AcceptableChild( entering, collisionWith ) ) && entering.myParent == null )
                {
                    // I am entering's parent
                    entering.myParent = GetComponent<LanguageObject>();
                    // entering is my child
                    myChildren.Add( entering );
                    // Make entering a child of me when the move is finished
                    GetComponent<MovableController>().additionalRelationshipChild = entering.transform;
                    GetComponent<MovableController>().additionalRelationshipParent = transform;
                    // Signal to outside entering has parent now
                    ( (ILanguageObjectListener) entering.GetComponent( typeof(ILanguageObjectListener) ) ).NewParent( GetComponent<LanguageObject>() );
                    // Signal to outside-me that I have a new child, which is entering
                    ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) ).NewChild( entering.GetComponent<LanguageObject>(), collisionWith );
                    
                    // entering has become my child.
                    if( myChuck != null && theirChuck == null )
                    {
                        // I will tell them they have a chuck now.
                        entering.TellChildrenHaveNewChuck( myChuck );
                    }
                }
            }
            else
            {
                // I have a parent
                myParent = entering;
                // And my parent has me
                myParent.myChildren.Add( GetComponent<LanguageObject>() );
                // Make me a child of the parent when the move is finished
                GetComponent<MovableController>().parentAfterMovement = myParent.transform;
                // Signal to outside I have a parent now
                ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) ).NewParent( myParent );
                // Signal to outside that it has a new child, me.
                ( (ILanguageObjectListener) entering.GetComponent( typeof(ILanguageObjectListener) ) ).NewChild( GetComponent<LanguageObject>(), collisionWith );
                
                // I have become entering's child
                if( myChuck == null && theirChuck != null )
                {
                    // I will tell my children that now we have a chuck!
                    TellChildrenHaveNewChuck( theirChuck );
                }
            }
        }
        
    }

    void LanguageObjectLastExited( LanguageObject removedFrom, Collider collisionWith )
    {
        // only listen to trigger exits if we are currently being moved
        if( GetComponent<MovableController>().amBeingMoved )
        {
            if( removedFrom == myParent )
            {
                RemoveFromParent();
            }
            else if( GetComponent<MovableController>().additionalRelationshipParent == transform &&
                     GetComponent<MovableController>().additionalRelationshipChild == removedFrom.transform )
            {
                // my child should no longer be added as an additional relationship
                removedFrom.RemoveFromParent();
                GetComponent<MovableController>().additionalRelationshipParent = null;
                GetComponent<MovableController>().additionalRelationshipChild = null;
            }
        }
    }

    public void RemoveFromParent()
    {
        ChuckInstance myChuck = GetChuck();
        if( !HaveOwnChuck() && myChuck != null )
        {
            // we don't have our own chuck, and we currently have access to one through
            // our parent. so, we will lose our chuck
            TellChildrenLosingChuck( myChuck );
        }
        // remove self from parent's children
        myParent.myChildren.Remove( this );
        // signal to outside that parent is being disconnected
        ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) ).ParentDisconnected( myParent );
        // signal to my parent that they are losing me as a child
        ( (ILanguageObjectListener) myParent.GetComponent( typeof(ILanguageObjectListener) ) ).ChildDisconnected( GetComponent<LanguageObject>() );
        // restore parent when the movement is finished to be the current "room", or top-level if we are top-level
        if( TheRoom.InAFunction() )
        {
            GetComponent<MovableController>().parentAfterMovement = TheRoom.GetCurrentFunction().GetBlockParent();
        }
        else
        {
            GetComponent<MovableController>().parentAfterMovement = null;
        }
        // don't have a parent
        myParent = null;
    }

    public static LanguageObject SearchForLanguageObject( Collider other )
    {
        Transform objToSearch = other.transform;
        while( objToSearch )
        {
            if( objToSearch.GetComponent<LanguageObject>() != null )
            {
                return objToSearch.GetComponent<LanguageObject>();
            }
            objToSearch = objToSearch.parent;
        }
        return null;
    }

    public ChuckInstance GetChuck()
    {
        if( HaveOwnChuck() )
        {
            return gameObject.GetComponent<ChuckInstance>();
        }
        else if( GetComponent<ControlWorldObjectController>() != null )
        {
            // things looking for chuck on a ControlWorldObjectController really
            // need the global chuck, which only has blackhole connections
            return TheChuck.Instance;
        }
        else if( myParent != null )
        {
            return myParent.GetChuck();
        }
        else
        {
            return null;
        }
    }

    public bool HaveOwnChuck()
    {
        return gameObject.GetComponent<ChuckInstance>() != null;
    }

    public void TellChildrenHaveNewChuck( ChuckInstance chuck )
    {
        ((ILanguageObjectListener) GetComponent(typeof(ILanguageObjectListener))).GotChuck( chuck );
        foreach( LanguageObject child in myChildren )
        {
            child.TellChildrenHaveNewChuck( chuck );
        }
    }

    public void TellChildrenLosingChuck( ChuckInstance chuck )
    {
        foreach( LanguageObject child in myChildren )
        {
            child.TellChildrenLosingChuck( chuck );
        }
        ((ILanguageObjectListener) GetComponent(typeof(ILanguageObjectListener))).LosingChuck( chuck );
    }
}


public interface ILanguageObjectListener
{
    bool AcceptableChild( LanguageObject other, Collider collisionWith );
    void NewParent( LanguageObject parent );
    void ParentDisconnected( LanguageObject parent );
    void NewChild( LanguageObject child, Collider collisionWith );
    void ChildDisconnected( LanguageObject child );
    string InputConnection();
    string OutputConnection();
    string VisibleName();
    void GotChuck( ChuckInstance chuck );
    void LosingChuck( ChuckInstance chuck );
}

public interface IDataSource
{
    float CurrentValue();
    float MinValue();
    float MaxValue();
    float NormValue();
}
