using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LanguageObject))]
[RequireComponent(typeof(SoundProducer))]
public class FunctionController : MonoBehaviour , ILanguageObjectListener, IParamAcceptor
{
    private static Dictionary< int, List< FunctionController > > allFunctions = null;

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

    private ChuckSubInstance myChuck = null;
    private string myStorageClass;
    private string myExitEvent;

    private ILanguageObjectListener myParent = null;

    public bool insideFunction = false;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        if( allFunctions == null )
        {
            allFunctions = new Dictionary< int, List< FunctionController > >();
        }

        myParams = new List<string>();
        myParamRefs = new List<FunctionParamController>();
        defaultParams = new string[] { "no valid params" };

        // init chuck
        myChuck = chuck;
        myStorageClass = myChuck.GetUniqueVariableName();
        myExitEvent = myChuck.GetUniqueVariableName();

        chuck.RunCode( string.Format( @"
            external Event {1};
            public class {0} 
            {{
                static Gain @ myOutGain;
                static Gain @ myInGain;
            }}

            Gain g1 @=> {0}.myOutGain;
            Gain g2 @=> {0}.myInGain;

            {1} => now;

        ", myStorageClass, myExitEvent ));
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        // TODO: necessary to Unhook my output from me?
        myChuck.BroadcastEvent( myExitEvent );
        myChuck = null;

        // cleanup object
        if( myFunctionId != -1 )
        {
            allFunctions[myFunctionId].Remove( this );
        }
    }

    // Use this for initialization
    void Start () {
		HookUpOutput();
        HookUpInput();
	}
	
	private void HookUpOutput()
    {
        // hook up output to me as a child.
        LanguageObject output = myOutput.GetComponent<LanguageObject>();
        LanguageObject me = GetComponent<LanguageObject>();

        // but only if it hasn't already been hooked up 
        if( output.myParent == null )
        {
            output.myParent = me;
            // output should ALWAYS be the first child
            me.myChildren.Insert( 0, output );
            myOutput.myFunction = this;

            // TODO: does this need to happen inside the if block or outside it?
            // hook up output block to MY output, so that output will be heard by outside the object
            myChuck.RunCode( string.Format( "{0} => {1};", myOutput.OutputConnection(), OutputConnection() ) );
        }
    }

    private void UnhookOutput()
    {
        // unhook up output to me as a child.
        LanguageObject output = myOutput.GetComponent<LanguageObject>();
        LanguageObject me = GetComponent<LanguageObject>();
        output.myParent = null;
        me.myChildren.Remove( output );

        // unhook up output block from MY output, so that output will not be heard by outside the object
        myChuck.RunCode( string.Format( "{0} =< {1};", myOutput.OutputConnection(), OutputConnection() ) );
    }

    private void HookUpInput()
    {
        LanguageObject input = myInput.GetComponent<LanguageObject>();
        LanguageObject me = GetComponent<LanguageObject>();

        // my input gain (which captures my args) --> my input block 
        //                 (--> other blocks --> my output block --> my output gain)
        myChuck.RunCode( string.Format( "{0} => {1};", InputConnection( input ), myInput.InputConnection( me ) ) );

    }

    private void UnhookInput()
    {
        LanguageObject input = myInput.GetComponent<LanguageObject>();
        LanguageObject me = GetComponent<LanguageObject>();

        // my input gain (which captures my args) --< my input block 
        myChuck.RunCode( string.Format( "{0} =< {1};", InputConnection( input ), myInput.InputConnection( me ) ) );
    }

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        myParent = parentListener;
        SwitchColors();
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        if( parentListener == myParent )
        {
            myParent = null;
            SwitchColors();
        }
    }
    
    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        if( other.GetComponent<SoundProducer>() != null || 
            ( other.GetComponent<ParamController>() != null && myParams.Count > 0 ) )
        {
            return true;
        }

        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( child.GetComponent<ParamController>() != null )
        {
            // TODO: If child is Param, it might need to be hooked up to be the child of the specific param
            // or else it will GotChuck too soon and try to connect to things that are not initialized yet...
        }
        else
        {
            LanguageObject.HookTogetherLanguageObjects( myChuck, child, myInput.GetComponent<LanguageObject>() );
        }
    }

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( child.GetComponent<ParamController>() != null )
        {
            // TODO: If child is Param, ...
        }
        else
        {
            LanguageObject.UnhookLanguageObjects( myChuck, child, myInput.GetComponent<LanguageObject>() );
        }
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        FunctionController other = original.GetComponent< FunctionController >();

        if( other.myFunctionId == -1 )
        {
            other.myFunctionId = APLIDSystem.GetNewID( "FunctionController" );
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
        Vector3 offsetToFunctionFloor = 1.5f * Vector3.up * GetComponent<MovableController>().GetScale();
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

        // tell my params that they might need to change what is valid
        RectifyOuterParams();

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
                // replace inner blocks
                fc.ReplaceInnerBlocks( myBlocks );
            }
        }
    }

    private void ReplaceInnerBlocks( GameObject newInnerBlocks )
    {
        // Tell TheRoom that the user is "inside" me so that function param blocks can find me
        TheRoom.EnterFunction( this );

        UnhookOutput();
        UnhookInput();
        ClearParams();
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
            // pass in localScale so that children of this clone will set their localScale correctly
            LanguageObject clonedChild = 
                childBlock.GetComponent< LanguageObject >().GetClone( childBlock.transform.localScale );
            // put it in the same position but inside me (this messes up localScale so reset it too)
            clonedChild.transform.parent = myBlocks.transform;
            clonedChild.transform.localScale = childBlock.transform.localScale;
            clonedChild.transform.localPosition = childBlock.transform.localPosition;
            clonedChild.transform.localRotation = childBlock.transform.localRotation;
        }
        FindOutput();
        HookUpOutput();
        FindInput();
        HookUpInput();
        RectifyOuterParams();

        // Tell TheRoom that the user is no longer "inside" me -- done copying
        TheRoom.ExitFunction( this );
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
        Queue<Transform> transformsToCheck = new Queue<Transform>();
        transformsToCheck.Enqueue( myBlocks.transform );
        while( transformsToCheck.Count > 0 )
        {
            Transform t = transformsToCheck.Dequeue();

            FunctionInputController maybeInput = t.GetComponent< FunctionInputController >();
            // if we found any input, it's ours!
            if( maybeInput != null )
            {
                myInput = maybeInput;
                myInput.myFunction = this;
                transformsToCheck.Clear();
                return;
            }

            FunctionController maybeFunction = t.GetComponent< FunctionController >();
            if( maybeFunction == null )
            {
                // check all non-function transforms recursively
                foreach( Transform child in t )
                {
                    transformsToCheck.Enqueue( child );
                }
            }
            // special handling for functions
            else
            {
                // we want to check all languageobject children of the function except for its outputcontroller
                foreach( LanguageObject child in maybeFunction.GetComponent<LanguageObject>().myChildren )
                {
                    if( child.GetComponent<FunctionOutputController>() == null )
                    {
                        transformsToCheck.Enqueue( child.transform );
                    }
                }
            }
        }
    }

    public void RectifyOuterParams()
    {
        foreach( LanguageObject child in GetComponent<LanguageObject>().myChildren )
        {
            ParamController maybeParam = child.GetComponent<ParamController>();
            if( maybeParam != null )
            {
                maybeParam.ResetAcceptableParams();
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

    

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }
    
    public string InputConnection( LanguageObject whoAsking )
    {
        return string.Format( "{0}.myInGain", myStorageClass );
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myOutGain", myStorageClass );
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

    public void ClearParams()
    {
        for( int i = myParamRefs.Count - 1; i >= 0; i-- )
        {
            RemoveParam( myParamRefs[i] );
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


                // Dirty hack: pass null as who is asking for the input connection
                // (hopefully, fn input blocks will never need it)
                myChuck.RunCode( string.Format("{0} => {1};", var, myParamRefs[i].InputConnection( null ) ));
            }
        }
    }

    public void DisconnectParam( string param, string var )
    {
        for( int i = 0; i < myParams.Count; i++ )
        {
            if( myParams[i] == param )
            {
                // Dirty hack: pass null as who is asking for the input connection
                // (hopefully, fn input blocks will never need it)
                myChuck.RunCode( string.Format("{0} =< {1};", var, myParamRefs[i].InputConnection( null ) ));
            }
        }
    }

    public string GetFunctionParentConnection( LanguageObject whoAsking )
    {
        return myParent.InputConnection( whoAsking );
    }

    public string VisibleName()
    {
        return myText.text;
    }

    public void SetTeleportationEnabled( bool enabled )
    {
        myTeleportationFloor.enabled = enabled;
    }

    public string[] SerializeStringParams( int version )
    {
        // no string params
        return LanguageObject.noStringParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // my function id
        return new int[] { myFunctionId };
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public object[] SerializeObjectParams( int version )
    {
        // serialize everything that is a child of myBlocks and a LanguageObject
        List<object> allChildBlocks = new List<object>();
        foreach( Transform child in myBlocks.transform )
        {
            LanguageObject maybeLanguageObject = child.GetComponent<LanguageObject>();
            if( maybeLanguageObject != null )
            {
                allChildBlocks.Add( maybeLanguageObject.SerializeObject() );
            }
        }
        return allChildBlocks.ToArray();
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams )
    {
        // retrieve function ID and add to static global storage
        myFunctionId = intParams[0];
        if( myFunctionId != -1 )
        {
            if( !allFunctions.ContainsKey( myFunctionId ) )
            {
                allFunctions[myFunctionId] = new List<FunctionController>();
            }
            allFunctions[myFunctionId].Add( this );
        }
        // in order to avoid overwriting existing function IDs,
        // declare this function ID is used
        APLIDSystem.DeclareIDUsed( "FunctionController", myFunctionId );

        // load inner blocks from serialization
        GameObject newMyBlocks = new GameObject();
        newMyBlocks.transform.localScale = Vector3.one;

        // Tell the room it's inside me so that blocks below are initialized correctly
        TheRoom.EnterFunction( this );

        // parent each old child to newMyBlocks with its original localTransform
        foreach( LanguageObjectSerialStorage storage in objectParams )
        {
            LanguageObject.DeserializeObject( storage, newMyBlocks );
        }

        // Done being inside me (though ReplaceInnerBlocks will do it again below)
        TheRoom.ExitFunction( this );

        // now, copy these blocks into myself
        ReplaceInnerBlocks( newMyBlocks );

        // and destroy the original
        Destroy( newMyBlocks );
    }
}
