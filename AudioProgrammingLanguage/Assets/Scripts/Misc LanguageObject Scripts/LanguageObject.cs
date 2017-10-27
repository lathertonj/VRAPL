using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(MovableController))]
[RequireComponent(typeof(RendererController))]
[RequireComponent(typeof(Rigidbody))]
public class LanguageObject : MonoBehaviour {

    // empty arrays for serialization
    public static string[] noStringParams = new string[0];
    public static int[] noIntParams = new int[0];
    public static float[] noFloatParams = new float[0];
    public static object[] noObjectParams = new object[0];

    public List<LanguageObject> myChildren;
    public LanguageObject myParent = null;
    public string prefabGeneratedFrom;

    private Dictionary<LanguageObject, int> currentCollisionCounts;
    private Dictionary<LanguageObject, int> enteringDebounceObjects;
    private Dictionary<LanguageObject, int> exitingDebounceObjects;
    private Dictionary<LanguageObject, Collider> enteringDebounceColliders;
    private Dictionary<LanguageObject, Collider> exitingDebounceColliders;
    private int debounceFramesToWait = 1;

    protected void Awake()
    {
        // recursively make sure all my colliders are trigger colliders and that my rigidbodies do not use gravity, etc
        EnsureTriggerBehavior( transform );
		myChildren = new List<LanguageObject>();
        currentCollisionCounts = new Dictionary<LanguageObject, int>();
        enteringDebounceObjects = new Dictionary<LanguageObject, int>();
        exitingDebounceObjects = new Dictionary<LanguageObject, int>();
        enteringDebounceColliders = new Dictionary<LanguageObject, Collider>();
        exitingDebounceColliders = new Dictionary<LanguageObject, Collider>();        
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
    protected virtual void Update () {
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
                .AcceptableChild( GetComponent<LanguageObject>() ) ) )
            {
                // "entering" will not consider me as a child

                // will I consider "entering" as a child if they don't already have a parent?
                // TODO: in this region, should "collisionWith" be passed as null? because the relationship
                // is flipped so I don't know what the actual collision on the parent was?
                if( ( ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) )
                    .AcceptableChild( entering ) ) && 
                    entering.myParent == null && 
                    ValidParentRelationship( entering, this ) )
                {
                    // I am entering's parent
                    entering.myParent = GetComponent<LanguageObject>();
                    // entering is my child
                    this.AddToChildren( entering );
                    // Make entering a child of me when the move is finished
                    GetComponent<MovableController>().additionalRelationshipChild = entering.transform;
                    GetComponent<MovableController>().additionalRelationshipParent = transform;
                    // Signal to outside entering has parent now
                    ( (ILanguageObjectListener) entering.GetComponent( typeof(ILanguageObjectListener) ) ).NewParent( GetComponent<LanguageObject>() );
                    // Signal to outside-me that I have a new child, which is entering
                    ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) ).NewChild( entering.GetComponent<LanguageObject>() );
                    
                    // entering has become my child.
                    if( myChuck != null && theirChuck == null )
                    {
                        // I will tell them they have a chuck now.
                        entering.TellChildrenHaveNewChuck( myChuck );
                    }
                }
            }
            // Entering will consider me as a child, but is this a valid relationship?
            else if( ValidParentRelationship( this, entering ) )
            {
                // I have a parent
                myParent = entering;
                // And my parent has me
                myParent.AddToChildren( this );
                // Make me a child of the parent when the move is finished
                GetComponent<MovableController>().parentAfterMovement = myParent.transform;
                // Signal to outside I have a parent now
                ( (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) ) ).NewParent( myParent );
                // Signal to outside that it has a new child, me.
                ( (ILanguageObjectListener) entering.GetComponent( typeof(ILanguageObjectListener) ) ).NewChild( GetComponent<LanguageObject>() );
                
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

    public virtual void RemoveFromParent()
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

    // avoid myParent loops cause by unwanted collisions when a program is cloned
    private bool ValidParentRelationship( LanguageObject child, LanguageObject parent )
    {
        while( parent != null )
        {
            if( parent == child )
            {
                return false;
            }
            parent = parent.myParent;
        }

        return true;
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

    public virtual ChuckInstance GetChuck()
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
        DacController maybeDac = gameObject.GetComponent<DacController>();
        if( maybeDac != null )
        {
            return maybeDac.IsEnabled() && maybeDac.GetComponent<ChuckInstance>() != null;
        }
        else
        {
            return gameObject.GetComponent<ChuckInstance>() != null;
        }
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

    public LanguageObject GetClone()
    {
        return GetClone( Vector3.one );
    }

    public LanguageObject GetClone( Vector3 localScale )
    {
        return GetCloneHelper( null, null, localScale );
    }

    public virtual void AddToChildren( LanguageObject child )
    {
        myChildren.Add( child );
    }

    private LanguageObject GetCloneHelper( LanguageObject parent, ILanguageObjectListener parentListener, 
        Vector3 localScale )
    {
        // copy myself
        GameObject copyGameObject = Instantiate( PrefabStorage.GetPrefab( prefabGeneratedFrom ), transform.position, transform.rotation );
        LanguageObject copy = copyGameObject.GetComponent<LanguageObject>();
        copy.prefabGeneratedFrom = prefabGeneratedFrom;
        ILanguageObjectListener copyListener = (ILanguageObjectListener) copy.GetComponent( typeof(ILanguageObjectListener) );
        
        ChuckInstance parentChuck = parent != null ? parent.GetChuck() : null;

        // make it a child of the parent
        if( parent != null )
        {
            // LanguageObject storage
            copy.myParent = parent;
            parent.AddToChildren( copy );

            // Unity transform tree
            copy.transform.parent = parent.transform;

            // notify the objects
            copyListener.NewParent( parent );
            parentListener.NewChild( copy );

            // do I have a chuck now?
            if( parentChuck != null )
            {
                // I will tell myself and my children that now we have a chuck!
                TellChildrenHaveNewChuck( parentChuck );
            }
        }

        // clone object-specific settings
        copyListener.CloneYourselfFrom( this, parent );
        
        // clone other settings such as size from MovableController and what else?
        copy.GetComponent< MovableController >().CloneFrom( this.GetComponent< MovableController >() );

        // copy size before children are copied: so that they have the correct localposition
        copy.transform.localScale = localScale;

        // clone each of my children and for each clone, make it be a child of copy
        foreach( LanguageObject child in myChildren )
        {
            // don't clone function output blocks here. they will get cloned as part of the function cloning process.
            if( child.GetComponent< FunctionOutputController >() != null )
            {
                continue;
            }
            LanguageObject clonedChild = child.GetCloneHelper( copy, copyListener, Vector3.one );
        }

        // reset renderer
        RendererController copyRenderer = copy.GetComponent<RendererController>();
        copyRenderer.Restart();

        return copy;
    }

    public LanguageObjectSerialStorage SerializeObject()
    {
        LanguageObjectSerialStorage myStorage = new LanguageObjectSerialStorage();
        ILanguageObjectListener myObject = (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) );
        myStorage.version = 0;

        // object specific params
        myStorage.stringParams = myObject.SerializeStringParams( myStorage.version );
        myStorage.intParams = myObject.SerializeIntParams( myStorage.version );
        myStorage.floatParams = myObject.SerializeFloatParams( myStorage.version );
        myStorage.objectParams = myObject.SerializeObjectParams( myStorage.version );

        // languageobject params
        myStorage.prefabName = prefabGeneratedFrom;
        myStorage.transformPosition = Serializer.SerializeVector3( transform.localPosition );
        myStorage.transformRotation = Serializer.SerializeQuaternion( transform.localRotation );
        myStorage.transformScale = Serializer.SerializeVector3( transform.localScale );
        MovableController mc = GetComponent<MovableController>();
        myStorage.languageSize = mc.GetScale();
        myStorage.minLanguageSize = mc.myMinScale;

        // children
        List<LanguageObjectSerialStorage> mySerializedChildren = new List<LanguageObjectSerialStorage>();
        for( int i = 0; i < myChildren.Count; i++ )
        {
            // don't serialize the function output -- it is a child of the function,
            // but it will be deserialized as part of the function's innards.
            if( myChildren[i].GetComponent<FunctionOutputController>() != null )
            {
                continue;
            }
            // serialize and store
            mySerializedChildren.Add( myChildren[i].SerializeObject() );
        }

        myStorage.children = mySerializedChildren.ToArray();

        return myStorage;
    }

    private static LanguageObject InstantiateLanguageObject( LanguageObjectSerialStorage storage, Transform parent )
    {
        GameObject copyGameObject = Instantiate( PrefabStorage.GetPrefab( storage.prefabName ), parent );
        // set localPosition and localRotation (Instantiate only allows setting global position and global rotation)
        copyGameObject.transform.localPosition = Serializer.DeserializeVector3( storage.transformPosition );
        copyGameObject.transform.localRotation = Serializer.DeserializeQuaternion( storage.transformRotation );
        return copyGameObject.GetComponent<LanguageObject>();
    }

    // NOTE: closely linked to GetClone() above!
    public static LanguageObject DeserializeObject( LanguageObjectSerialStorage storage )
    {
        // copy myself
        LanguageObject me = InstantiateLanguageObject( storage, null );
        // deserialize
        me.DeserializeObjectHelper( storage, null, null );
        // tada!!
        return me;
    }

    public static LanguageObject DeserializeObject( LanguageObjectSerialStorage storage, GameObject parent )
    {
        // deserialize
        LanguageObject me = DeserializeObject( storage );
        
        // store local transform properties (these get changed when parent is changed)
        Vector3 myLocalScale = me.transform.localScale;
        Vector3 myLocalPosition = me.transform.localPosition;
        Quaternion myLocalRotation = me.transform.localRotation;
        
        // set parent
        me.transform.parent = parent.transform;

        // reset local transform properties
        me.transform.localScale = myLocalScale;
        me.transform.localPosition = myLocalPosition;
        me.transform.localRotation = myLocalRotation;

        // send off into the world...
        return me;
    }

    private void DeserializeObjectHelper( LanguageObjectSerialStorage storage,
        LanguageObject parent, ILanguageObjectListener parentListener )
    {
        prefabGeneratedFrom = storage.prefabName;
        ILanguageObjectListener meListener = (ILanguageObjectListener) GetComponent( typeof(ILanguageObjectListener) );
        
        ChuckInstance parentChuck = ( parent != null ) ? parent.GetChuck() : null;

        // make it a child of the parent
        if( parent != null )
        {
            // LanguageObject storage
            myParent = parent;
            parent.AddToChildren( this );

            // notify the objects
            meListener.NewParent( parent );
            parentListener.NewChild( this );
        }

        // clone object-specific settings
        meListener.SerializeLoad( storage.version, storage.stringParams, storage.intParams, 
            storage.floatParams, storage.objectParams );

        // do I have a chuck now?
        if( parentChuck != null )
        {
            // I will tell myself and my children that now we have a chuck!
            // (children are deserialized below, so this is just really myself
            //  and any children created via SerializeLoad(), such as
            //  a function's FunctionOutputController)
            TellChildrenHaveNewChuck( parentChuck );
        }
        
        // clone other settings such as size from MovableController and what else?
        MovableController mc = GetComponent<MovableController>();
        mc.myMinScale = storage.minLanguageSize;
        mc.SetScale( storage.languageSize );

        // copy size before children are copied: so that they have the correct localposition
        transform.localScale = Serializer.DeserializeVector3( storage.transformScale );

        // deserialize all children
        foreach( LanguageObjectSerialStorage childStorage in storage.children )
        {
            // TODO: special handling for FunctionOutputController???
            LanguageObject newChild = InstantiateLanguageObject( childStorage, this.transform );
            newChild.DeserializeObjectHelper( childStorage, this, meListener );
        }

        // reset renderer (necessary?)
        RendererController renderer = GetComponent<RendererController>();
        renderer.Restart();
    }
}


public interface ILanguageObjectListener
{
    bool AcceptableChild( LanguageObject other );
    void NewParent( LanguageObject parent );
    void ParentDisconnected( LanguageObject parent );
    void NewChild( LanguageObject child );
    void ChildDisconnected( LanguageObject child );
    string VisibleName();
    void GotChuck( ChuckInstance chuck );
    void LosingChuck( ChuckInstance chuck );
    string InputConnection( LanguageObject whoAsking );
    string OutputConnection();
    void SizeChanged( float newSize );
    void CloneYourselfFrom( LanguageObject original, LanguageObject newParent );

    // serialization for storage on disk
    string[] SerializeStringParams( int version );
    int[] SerializeIntParams( int version );
    float[] SerializeFloatParams( int version );
    object[] SerializeObjectParams( int version );
    void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams );
}

[Serializable]
public struct LanguageObjectSerialStorage
{
    // populated by the custom class / interface
    public string[] stringParams;
    public int[] intParams;
    public float[] floatParams;
    public object[] objectParams;

    // populated by LanguageObject
    public int version;
    public string prefabName;
    public LanguageObjectSerialStorage[] children;
    public float[] transformScale;
    public float[] transformPosition;
    public float[] transformRotation;
    public float languageSize;
    public float minLanguageSize;
}

public interface IDataSource
{
    float CurrentValue();
    float MinValue();
    float MaxValue();
    float NormValue();
}
