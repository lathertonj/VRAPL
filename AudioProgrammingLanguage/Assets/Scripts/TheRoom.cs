using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheRoom : MonoBehaviour {

    public static Transform theRoom;
    public static Transform theEye;
    public static MeshCollider theGround;
    public static GameObject theFunctionGenerators;
    public Transform room;
    public Transform eye;
    public GameObject functionGenerators;
    public MeshCollider ground;

    private static Stack<FunctionController> functionsIAmIn;

	void Awake () {
		theRoom = room;
        theEye = eye;
        theGround = ground;
        theFunctionGenerators = functionGenerators;

        functionsIAmIn = new Stack<FunctionController>();
	}

    public static void EnterFunction( FunctionController fc )
    {
        if( InAFunction() )
        {
            functionsIAmIn.Peek().SetTeleportationEnabled( false );
        }
        functionsIAmIn.Push( fc );
        fc.SetTeleportationEnabled( true );
        ShowOrHideFunctionGenerators();
        UpdateGroundTeleportability();
    }

    public static void ExitFunction( FunctionController fc )
    {
        if( fc != functionsIAmIn.Peek() )
        {
            Debug.LogError( "TheRoom exiting a function out of order" );
            return;
        }
        functionsIAmIn.Pop();
        fc.SetTeleportationEnabled( false );
        if( InAFunction() )
        {
            functionsIAmIn.Peek().SetTeleportationEnabled( true );
        }
        ShowOrHideFunctionGenerators();
        UpdateGroundTeleportability();
    }

    private static void ShowOrHideFunctionGenerators()
    {
        theFunctionGenerators.SetActive( InAFunction() );
    }

    private static void UpdateGroundTeleportability()
    {
        theGround.enabled = ! InAFunction();
    }

    public static bool InAFunction()
    {
        return functionsIAmIn.Count > 0;
    }

    public static FunctionController GetCurrentFunction()
    {
        if( InAFunction() )
        { 
            return functionsIAmIn.Peek();
        }
        else
        {
            return null;
        }
    }
}
