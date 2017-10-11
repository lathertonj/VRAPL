using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PrefabStorage : MonoBehaviour {

    public static PrefabStorage thePrefabs;

    public NamedPrefab[] allPrefabs;

	// Use this for initialization
	void Awake () {
        // singleton
		if( thePrefabs == null )
        {
            thePrefabs = this;
        }
        else if( thePrefabs != this )
        {
            Destroy( gameObject );
        }
	}
	
	
}

[Serializable]
public struct NamedPrefab
{
    public string name;
    public GameObject prefab;
}
