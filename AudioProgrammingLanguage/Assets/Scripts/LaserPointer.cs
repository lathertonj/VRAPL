using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {


    // showing laser
    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;

    // teleporting
    public Transform cameraRigTransform;
    public GameObject teleportTargetPrefab;
    private GameObject teleportTarget;
    private Transform teleportTargetTransform;
    public Transform headTransform;
    public Vector3 teleportTargetOffset;
    public LayerMask teleportMask;
    private bool shouldTeleport;
    private PickUpObjects myGrabber;

    // TODO: don't teleport when sending signals to a language object

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input( (int) trackedObj.index ); }
    }

    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private void Start () {
		laser = Instantiate( laserPrefab, TheRoom.theRoom );
        laserTransform = laser.transform;
        teleportTarget = Instantiate( teleportTargetPrefab, TheRoom.theRoom );
        teleportTargetTransform = teleportTarget.transform;
        myGrabber = GetComponent<PickUpObjects>();
        teleportTargetOffset = 0.01f * Vector3.up;
	}
	
	private void Update () {
		if( Controller.GetPress( SteamVR_Controller.ButtonMask.Touchpad ) && !myGrabber.UsingTrackpad() )
        {
            RaycastHit hit;

            if( Physics.Raycast( trackedObj.transform.position, transform.forward, out hit, 100, teleportMask ) )
            {
                hitPoint = hit.point;
                ShowLaser( hit );

                teleportTarget.SetActive( true );
                teleportTargetTransform.position = hitPoint + teleportTargetOffset;
                shouldTeleport = true;
            }
            else
            {
                shouldTeleport = false;
                laser.SetActive( false );
                teleportTarget.SetActive( false );
            }
        }
        else
        {
            laser.SetActive( false );
            teleportTarget.SetActive( false );
        }

        if( Controller.GetPressUp( SteamVR_Controller.ButtonMask.Touchpad ) && shouldTeleport && !myGrabber.UsingTrackpad() )
        {
            Teleport();
        }
	}

    private void ShowLaser( RaycastHit hit )
    {
        laser.SetActive( true );
        laserTransform.position = Vector3.Lerp( trackedObj.transform.position, hitPoint, .5f );
        laserTransform.LookAt( hitPoint );
        laserTransform.localScale = new Vector3( laserTransform.localScale.x, laserTransform.localScale.y, hit.distance );
    }

    private void Teleport()
    {
        shouldTeleport = false;
        teleportTarget.SetActive( false );
        // move so that user's head stands where the laser hit point was
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        difference.y = 0;
        cameraRigTransform.position = hitPoint + difference;
    }
}
