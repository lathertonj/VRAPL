using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {

	public GameObject prefab;

    public GameObject GetCopy()
    {
        return Instantiate(prefab, transform.position, transform.rotation);
    }
}
