using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSubChuck : MonoBehaviour
{
    public static ChuckSubInstance instance = null;

    void Awake()
    {
        if( instance == null )
        {
            instance = GetComponent<ChuckSubInstance>();
        }
    }
}
