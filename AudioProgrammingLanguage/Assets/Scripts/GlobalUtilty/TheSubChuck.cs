using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSubChuck : MonoBehaviour {

    public static ChuckSubInstance Instance;

    // Use this for initialization
    private void Awake()
    {
        Instance = GetComponent<ChuckSubInstance>();
        Instance.chuckMainInstance = TheMainChuck.Instance;
    }
}
