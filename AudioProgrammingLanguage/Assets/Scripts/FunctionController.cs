﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(SoundProducer))]
public class FunctionController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor
{
    private static Dictionary< int, List< FunctionController > > allFunctions = null;
    private static int currentFunctionId = 0;

    public FunctionInputController myInput;
    public FunctionOutputController myOutput;
    public Transform myBlocksHolder;
    public GameObject myBlocks;
    public Light myLight;
    public Collider myTeleportationFloor;

    // for color switching
    public TextMesh myText;
    public MeshRenderer[] myShapes;

    // for entering and exiting
    private Vector3 lastPosition;
    private Vector3 lastOrientation;
    private Vector3 lastRoomPosition;
    private Vector3 lastHeadPosition;
    private Vector3 enterDirection;
    private float lastScale;

    // for params
    private List<string> myParams;
    private List<FunctionParamController> myParamRefs;
    private int numParams = 0;
    private string[] defaultParams;

    // for copying changes over to other functions made out of this one
    private int myFunctionId = -1;

    private ChuckInstance myChuck = null;

    private ILanguageObjectListener myParent = null;

    public bool insideFunction = false;

    private void Awake()
    {
        if( allFunctions == null )
        {
            allFunctions = new Dictionary< int, List< FunctionController > >();
        }

        myParams = new List<string>();
        myParamRefs = new List<FunctionParamController>();
        defaultParams = new string[] { "no valid params" };
    }

    // Use this for initialization
    void Start () {
		HookUpOutput();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void HookUpOutput()
    {
        // hook up output to me as a child.
        LanguageObject output = myOutput.GetComponent<LanguageObject>();
        LanguageObject me = GetComponent<LanguageObject>();
        output.myParent = me;
        me.myChildren.Add( output );
        myOutput.myFunction = this;
    }

    private void UnhookOutput()
    {
        // hook up output to me as a child.
        LanguageObject output = myOutput.GetComponent<LanguageObject>();
        LanguageObject me = GetComponent<LanguageObject>();
        output.myParent = null;
        me.myChildren.Remove( output );
    }

    /*
    public GameObject Clone()
    {
        if( myFunctionId == -1 )
        {
            myFunctionId = currentFunctionId;
            currentFunctionId++;
            allFunctions[ myFunctionId ] = new List< FunctionController >();
            allFunctions[ myFunctionId ].Add( this );
        }

        GameObject copy = Instantiate( gameObject );
        copy.GetComponent< RendererController >().Restart();
        FunctionController copiedFunction = copy.GetComponent< FunctionController >();
        copiedFunction.myFunctionId = myFunctionId;
        allFunctions[ myFunctionId ].Add( copiedFunction );

        return copy;
    }
    */

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        FunctionController other = original.GetComponent< FunctionController >();

        if( other.myFunctionId == -1 )
        {
            other.myFunctionId = currentFunctionId;
            currentFunctionId++;
            allFunctions[ other.myFunctionId ] = new List< FunctionController >();
            allFunctions[ other.myFunctionId ].Add( other );
        }
        this.myFunctionId = other.myFunctionId;
        allFunctions[ myFunctionId ].Add( this );

        // call UpdateClones... on other; all functions with the same ID as other
        // will get their insides replicated
        other.UpdateClonesInnerBlocks();
    }

    private void SwitchColors()
    {
        Color temp = myText.color;
        myText.color = myShapes[0].material.color;
        foreach( MeshRenderer m in myShapes )
        {
            m.material.color = temp;
        }
    }

    public void EnterFunction()
    {
        insideFunction = true; 

        enterDirection = transform.position - TheRoom.theEye.position;
        
        // increase world size by 10
        WorldSize.changeWorldSize( 10 );

        // set my scale to 1 so the room will be ok for walking around in
        lastScale = GetComponent<MovableController>().GetScale();
        GetComponent<MovableController>().SetScale( 1f );

        // set my orientation to 0 along x and z (TODO: will this cause problems with connections?)
        lastOrientation = transform.eulerAngles;
        transform.eulerAngles = new Vector3( 0, lastOrientation.y, 0 );

        // reposition person to be here too
        lastRoomPosition = TheRoom.theRoom.position;
        lastHeadPosition = TheRoom.theEye.localPosition;
        // position person (not room) to be at the center of the function
        Vector3 roomEyeDifference = TheRoom.theRoom.position - TheRoom.theEye.position;
        roomEyeDifference.y = 0;
        // lower person so they are standing on the function floor
        Vector3 offsetToFunctionFloor = 1.5f * Vector3.up * GetComponent<MovableController>().myScale;
        // combine offsets and set
        TheRoom.theRoom.position = transform.position - offsetToFunctionFloor + roomEyeDifference;
        TheRoom.EnterFunction( this );

        // turn on my light
        myLight.enabled = true;
    }

    public void ExitFunction()
    {
        insideFunction = false;

        // make world smaller
        WorldSize.changeWorldSize( 0.1f );

        // reset my scale
        GetComponent<MovableController>().SetScale( lastScale );

        // reset my orientation
        transform.eulerAngles = lastOrientation;

        // turn off my light
        myLight.enabled = false;
        
        // position person back in front of the function
        Vector3 newPosition = transform.position - 3 * enterDirection - TheRoom.theEye.localPosition;
        newPosition.y = lastRoomPosition.y;
        TheRoom.theRoom.position = newPosition;


        TheRoom.ExitFunction( this );

        // update all other functions of this type
        UpdateClonesInnerBlocks();
    }

    private void UpdateClonesInnerBlocks()
    {
        if( myFunctionId == -1 )
        {
            // there are no clones of me
            return;
        }

        foreach( FunctionController fc in allFunctions[ myFunctionId ] )
        {
            // only replace ones other than myself
            if( fc != this )
            {
                fc.ReplaceInnerBlocks( myBlocks );
            }
        }
    }

    private void ReplaceInnerBlocks( GameObject newInnerBlocks )
    {
        UnhookOutput();
        Destroy( myBlocks );
        myBlocks = new GameObject();
        myBlocks.transform.parent = myBlocksHolder;
        // center and correct size it
        myBlocks.transform.localScale = Vector3.one;
        myBlocks.transform.localEulerAngles = Vector3.zero;
        myBlocks.transform.localPosition = Vector3.zero;
        // myBlocks = Instantiate( newInnerBlocks, myBlocksHolder );
        foreach( Transform childBlock in newInnerBlocks.transform )
        {
            LanguageObject clonedChild = 
                childBlock.GetComponent< LanguageObject >().GetClone( childBlock.transform.localScale );
            // put it in the same position but inside me
            clonedChild.transform.parent = myBlocks.transform;
            clonedChild.transform.localPosition = childBlock.transform.localPosition;
            clonedChild.transform.localRotation = childBlock.transform.localRotation;
        }
        FindOutput();
        HookUpOutput();
        FindInput();
    }

    private void FindOutput()
    {
        foreach( Transform child in myBlocks.transform )
        {
            FunctionOutputController maybeOutput = child.GetComponent< FunctionOutputController >();
            if( maybeOutput != null )
            {
                myOutput = maybeOutput;
                myOutput.myFunction = this;
                return;
            }
        }
    }

    private void FindInput()
    {
        foreach( Transform child in myBlocks.transform )
        {
            FunctionInputController maybeInput = child.GetComponent< FunctionInputController >();
            if( maybeInput != null )
            {
                myInput = maybeInput;
                myInput.myFunction = this;
                return;
            }
        }
    }
    
    public Transform GetBlockParent()
    {
        return myBlocks.transform;
    }


    public void HeadEnteredPortal()
    {
        if( insideFunction )
        {
            ExitFunction();
        }
        else
        {
            EnterFunction();
        }
    }

    public bool AcceptableChild( LanguageObject other )
    {
        if( other.GetComponent<SoundProducer>() != null || 
            ( other.GetComponent<ParamController>() != null && myParams.Count > 0 ) )
        {
            return true;
        }

        return false;
    }

    public void NewParent( LanguageObject parent )
    {
        Debug.Log("getting a parent... hopefully!");
        ILanguageObjectListener newParent = (ILanguageObjectListener) parent.GetComponent( typeof( ILanguageObjectListener ) );
        if( newParent != null )
        {
            myParent = newParent;
            Debug.Log(gameObject.name + " is setting my parent to " + parent.gameObject.name );
            SwitchColors();
        }
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        ILanguageObjectListener losingParent = (ILanguageObjectListener) parent.GetComponent( typeof( ILanguageObjectListener ) );
        if( losingParent == myParent )
        {
            myParent = null;
            SwitchColors();
        }
    }
    
    public void NewChild( LanguageObject child )
    {
        // TODO: If child is Param, it might need to be hooked up to be the child of the specific param
        // or else it will GotChuck too soon and try to connect to things that are not initialized yet...
    }

    public void ChildDisconnected( LanguageObject child )
    {
        // TODO: if child is param, ...
    }

    public void GotChuck( ChuckInstance chuck )
    {
        myChuck = chuck;
        Debug.Log("I am " + gameObject.name);
        Debug.Log("my children who will be getting chuck are:");
        foreach( LanguageObject child in GetComponent<LanguageObject>().myChildren)
        {
            Debug.Log(child.gameObject.name );
        }
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        myChuck = null;
    }
    
    public string InputConnection()
    {
        // TODO: return some error if myInput doesn't have a chuck?
        return myInput.InputConnection();
    }

    public string OutputConnection()
    {
        return myOutput.OutputConnection();
    }

    public string AddParam( FunctionParamController newParam, string name )
    {
        if( name == "" )
        {
            numParams++;
            name = string.Format( "parameter {0}", numParams );
        }
        else
        {
            bool nameInUse = false;
            for( int i = 0; i < myParams.Count; i++ )
            {
                if( myParams[i] == name )
                {
                    nameInUse = true;
                    break;
                }
            }
            if( nameInUse )
            {
                name += string.Format( " {0}", numParams );
            }
        }

        myParams.Add( name );
        myParamRefs.Add( newParam );

        return name;
    }

    public void RemoveParam( FunctionParamController removeParam )
    {
        for( int i = 0; i < myParamRefs.Count; i++ )
        {
            if( myParamRefs[i] == removeParam )
            {
                myParams.RemoveAt( i );
                myParamRefs.RemoveAt( i );
                return;
            }
        }
    }

    public string[] AcceptableParams()
    {
        if( myParams.Count > 0 )
        {   
            return myParams.ToArray();
        }
        else
        {
            return defaultParams;
        }
    }

    public void ConnectParam( string param, string var )
    {
        for( int i = 0; i < myParams.Count; i++ )
        {
            if( myParams[i] == param )
            {
                // this works because output is always the first child, and so the
                // inner param block will be initialized first even though it and
                // the outer param block are both descendants of the same block 
                myChuck.RunCode( string.Format("{0} => {1};", var, myParamRefs[i].InputConnection() ));
            }
        }
    }

    public void DisconnectParam( string param, string var )
    {
        for( int i = 0; i < myParams.Count; i++ )
        {
            if( myParams[i] == param )
            {
                myChuck.RunCode( string.Format("{0} =< {1};", var, myParamRefs[i].InputConnection() ));
            }
        }
    }

    public string GetFunctionParentConnection()
    {
        return myParent.InputConnection();
    }

    public string VisibleName()
    {
        return myText.text;
    }

    public void SetTeleportationEnabled( bool enabled )
    {
        myTeleportationFloor.enabled = enabled;
    }

    // helper functions called by my Input block
    public void TellUgenChildrenGotChuck( ChuckInstance chuck )
    {
        ArrayList myChildren = GetComponent< LanguageObject >().myChildren;
        foreach( LanguageObject child in myChildren )
        {
            // don't need to notify my Output block or any of my param blocks
            // since they are not connected into my Input block
            if( child.GetComponent< FunctionOutputController >() != null ||
                child.GetComponent< ParamController >() != null )
            {
                continue;
            }
            child.TellChildrenHaveNewChuck( chuck );
        }
    }

    public void TellUgenChildrenLosingChuck( ChuckInstance chuck )
    {
        ArrayList myChildren = GetComponent< LanguageObject >().myChildren;
        foreach( LanguageObject child in myChildren )
        {
            // don't need to notify my Output block or any of my param blocks
            // since they are not connected into my Input block
            if( child.GetComponent< FunctionOutputController >() != null ||
                child.GetComponent< ParamController >() != null )
            {
                continue;
            }
            child.TellChildrenLosingChuck( chuck );
        }
    }

}
