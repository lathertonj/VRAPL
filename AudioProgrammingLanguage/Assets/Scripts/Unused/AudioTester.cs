using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<ChuckSubInstance>().RunCode(
            @"
            TriOsc foo => dac;
            while( true )
            {
                Math.random2f(300, 1000) => foo.freq;
                chout <= foo.freq() <= IO.newline();
                100::ms => now;
            }
            "
        );
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
