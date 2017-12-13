using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APLIDSystem : MonoBehaviour {
    
    public static int GetNewID( string type )
    {
        EnsureEntryExists( type );

        // get
        int newID = nextIDs[type];

        // increment for next time
        nextIDs[type]++;

        return newID;
    }

    public static void DeclareIDUsed( string type, int usedID )
    {
        EnsureEntryExists( type );

        // next should be the larger of the current next and usedID + 1
        nextIDs[type] = System.Math.Max( nextIDs[type], usedID + 1 );
    }

    private static void EnsureEntryExists( string type )
    {
        // init dict
        if( nextIDs == null )
        {
            nextIDs = new Dictionary<string, int>();
        }

        // init type storage
        if( !nextIDs.ContainsKey( type ) )
        {
            nextIDs[type] = 0;
        }
    }

    private static Dictionary<string, int> nextIDs;
}
