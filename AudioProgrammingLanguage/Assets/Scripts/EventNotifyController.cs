using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventNotifyController : MonoBehaviour {

    private List<EventNotifyController> myListeners;
    private IEventNotifyResponder myResponder = null;

    private void Start()
    {
        myListeners = new List<EventNotifyController>();
        myResponder = (IEventNotifyResponder) GetComponent( typeof( IEventNotifyResponder ) );
    }

    public void AddListener( EventNotifyController e )
    {
        myListeners.Add( e );
    }

    public void RemoveListener( EventNotifyController e )
    {
        myListeners.Remove( e );
    }

	public void TriggerEvent( float intensity )
    {
        // check again if it exists now
        if( myResponder == null )
        {
            myResponder = (IEventNotifyResponder) GetComponent( typeof( IEventNotifyResponder ) );
        }

        // using what we found
        if( myResponder != null )
        {
            myResponder.RespondToEvent( intensity );
        }

        foreach( EventNotifyController child in myListeners )
        {
            child.TriggerEvent( intensity );
        }
    }
}

public interface IEventNotifyResponder
{
    void RespondToEvent( float intensity );
}
