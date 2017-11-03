using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheMainChuck : MonoBehaviour {

    public static ChuckMainInstance Instance;

	// Use this for initialization
	void Awake()
    {
		Instance = GetComponent<ChuckMainInstance>();
	}
}
