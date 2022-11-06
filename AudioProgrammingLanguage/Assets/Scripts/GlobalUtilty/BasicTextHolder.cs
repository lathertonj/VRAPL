using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTextHolder : MonoBehaviour {

    public static Transform basicTextPrefab;
    public Transform setBasicTextPrefab;

	void Start () {
		basicTextPrefab = setBasicTextPrefab;
	}

}
