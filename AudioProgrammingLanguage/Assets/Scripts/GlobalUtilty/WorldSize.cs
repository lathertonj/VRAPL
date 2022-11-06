using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSize : MonoBehaviour 
{

    public static float currentWorldSize = 1f;

	public static void changeWorldSize( float factor )
    {
        if( factor < 0.001f )
        {
            Debug.Log( "not changing world size by less than 1/1000th" );
            return;
        }

        // change the float, which should be used by anything that adds force or velocity
        currentWorldSize *= factor;

        // change gravity
        Physics.gravity *= factor;

        // change the size of all objects in the scene that have no parent, except for TheRoom.theRoom
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        for( int i = 0; i < roots.Length; i++ )
        {
            // enlarge everything but the main room
            if( roots[i] != TheRoom.theRoom.gameObject )
            {
                // make it further away
                roots[i].transform.position *= factor;

                // make it bigger
                roots[i].transform.localScale *= factor;
            }
        }
    }


}
