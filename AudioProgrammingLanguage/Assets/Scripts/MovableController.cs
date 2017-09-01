    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableController : MonoBehaviour {

	public bool amBeingMoved = false;
    public bool amMovable = true;
    public Transform amBeingMovedBy = null;
    public Transform parentAfterMovement = null;
    public Transform additionalRelationshipChild = null;
    public Transform additionalRelationshipParent = null;

    public float myScale = 1.0f;
    public float myMinScale = 1.0f;
    
    // only want to scale up and down the transforms that were part of me at the time
    // of my construction, so that inheriting language objects don't get changed too.
    private Transform[] myInitialChildren;
    private Vector3[] myInitialChildrenBaseScales;

    private void Start()
    {
        myInitialChildren = new Transform[transform.childCount];
        myInitialChildrenBaseScales = new Vector3[transform.childCount];

        for( int i = 0; i < transform.childCount; i++ )
        {
            myInitialChildren[i] = transform.GetChild( i );
            myInitialChildrenBaseScales[i] = myInitialChildren[i].localScale / myScale;
        }
    }

    private void Update()
    {
        for( int i = 0; i < myInitialChildren.Length; i++ )
        {
            myInitialChildren[i].localScale = myInitialChildrenBaseScales[i] * myScale;
        }
    }

    public void SetScale( float s )
    {
        myScale = Mathf.Max( s, myMinScale );
    }

    public float GetScale()
    {
        return myScale;
    }

}
