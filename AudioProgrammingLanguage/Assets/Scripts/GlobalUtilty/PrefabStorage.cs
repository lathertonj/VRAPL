using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PrefabStorage : MonoBehaviour {

    public static PrefabStorage thePrefabs;

    public NamedPrefab[] allPrefabs;

    private Dictionary<string, GameObject> myPrefabs;
    private Dictionary<GameObject, string> myNames;

	// Use this for initialization
	void Awake () {
        // singleton
		if( thePrefabs == null )
        {
            thePrefabs = this;
            myPrefabs = new Dictionary<string, GameObject>();
            myNames = new Dictionary<GameObject, string>();

            for( int i = 0; i < allPrefabs.Length; i++ )
            {
                myPrefabs[allPrefabs[i].name] = allPrefabs[i].prefab;
                myNames[allPrefabs[i].prefab] = allPrefabs[i].name;
            }
        }
        else if( thePrefabs != this )
        {
            Destroy( gameObject );
        }
	}
	
	public static GameObject GetPrefab( string name )
    {
        if( !thePrefabs.myPrefabs.ContainsKey( name ) )
        {
            Debug.LogError( "I don't know what is prefab: " + name );
        }
        return thePrefabs.myPrefabs[name];
    }

    public static string GetName( GameObject prefab )
    {
        return thePrefabs.myNames[prefab];
    }
}

[Serializable]
public struct NamedPrefab
{
    public string name;
    public GameObject prefab;
}
