using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ChuckGetterExample : MonoBehaviour
{

	public AudioMixer mixerWithChuck;
	private string myChuck;

	private double foo;
	private bool gotNewFoo;

	/*void Start()
	{
		gotNewFoo = false;

		myChuck = "nonspatial_chuck";
		Chuck.Manager.Initialize( mixerWithChuck, myChuck );
		Chuck.Manager.RunCode( myChuck, 
			@"
			external float foo;
			
			while( true )
			{
				Math.random2f( 0, 100 ) => foo;
				10::ms => now;
			}
			"
		);
	}
	
	void Update()
	{
		if( gotNewFoo )
		{
			//Debug.Log( "new foo got from chuck: " + foo.ToString() );
			gotNewFoo = false;
		}
	}

	void OnCollisionEnter( Collision other )
	{
		// ask for foo when I collide with things
		Chuck.Manager.GetFloat( myChuck, "foo", GetFooCallback );
	}

	void GetFooCallback( double newVal )
	{
		foo = newVal;
		gotNewFoo = true;
	}*/
}
