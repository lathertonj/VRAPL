using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldTheChuck : MonoBehaviour {

    public static ChuckInstance Instance;

	void Awake () {
		Instance = GetComponent<ChuckInstance>();
	}

}
