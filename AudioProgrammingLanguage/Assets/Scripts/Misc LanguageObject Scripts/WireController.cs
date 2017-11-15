using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireController : MonoBehaviour {

    private MeshRenderer myRenderer;
    private float myXScale;
    private float myZScale;

    private Transform myStart;
    private Transform myEnd;


    private void Awake()
    {
        myRenderer = GetComponent<MeshRenderer>();
        myXScale = transform.localScale.x;
        myZScale = transform.localScale.z;
        UpdateTextureTiling();

        myStart = transform;
        myEnd = transform;
    }

    private void Update()
    {
        // update to stay connected to myStart and myEnd
        SetEndpoints( myStart.position, myEnd.position );
    }

    public void SetEndpoints( Transform start, Transform end )
    {
        myStart = start;
        myEnd = end;
    }

    private void SetEndpoints( Vector3 start, Vector3 end )
    {
        // compute directional vector
        Vector3 offset = end - start;

        // set angle
        transform.up = offset;

        // set position
        transform.position = start + offset / 2;

        // set scale: 
        transform.localScale = new Vector3( myXScale, offset.magnitude / 2, myZScale );
        UpdateTextureTiling();
    }

    private void UpdateTextureTiling()
    {
        myRenderer.material.mainTextureScale = new Vector2( 1, transform.localScale.y * 50 );
    }
}
