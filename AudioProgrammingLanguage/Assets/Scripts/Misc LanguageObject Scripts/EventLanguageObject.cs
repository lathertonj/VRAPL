using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLanguageObject : LanguageObject {

    IEventLanguageObjectListener myMaybeListener = null;
    IEventLanguageObjectEmitter myMaybeEmitter = null;
    List<EventLanguageObject> myEventChildren = null;
    EventLanguageObject myEventParent = null;
    Chuck.VoidCallback myTriggerCallback = null;

    string myListeningTriggerEvent = "";
    int myTriggerCount = 0;

    private new void Awake()
    {
        base.Awake();
        myMaybeListener = (IEventLanguageObjectListener) GetComponent( typeof(IEventLanguageObjectListener) );
        myMaybeEmitter = (IEventLanguageObjectEmitter) GetComponent( typeof(IEventLanguageObjectEmitter) );
        if( myMaybeListener == null && myMaybeEmitter == null )
        {
            Debug.LogError( "EventLanguageObject must at least implement Listener or Emitter!" );
        }
        myEventChildren = new List<EventLanguageObject>();
        myTriggerCallback = Chuck.CreateVoidCallback( ListenTriggerCallback );
    }

    private void Start()
    {
        // if I'm an emitter, set me up
        if( myMaybeEmitter != null )
        {
            myMaybeEmitter.StartEmitTrigger();
        }
    }

    protected override void Update()
    {
        // do LanguageObject update
        base.Update();

        // if I've received any triggers, send them out to my listener
        while( myTriggerCount > 0 )
        {
            myTriggerCount--;
            if( myMaybeListener != null )
            {
                myMaybeListener.TickDoAction();
            }
        }
    }

    public override void AddToChildren( LanguageObject child )
    {
        // LanguageObject storage
        base.AddToChildren( child );

        // EventLanguageObject storage and reactions
        if( child is EventLanguageObject )
        {
            // populate storage tree
            EventLanguageObject eventChild = (EventLanguageObject) child;
            myEventChildren.Add( eventChild );
            eventChild.myEventParent = this;
            
            // tell it what to listen to: find the first emitter in the parent tree
            IEventLanguageObjectEmitter parentEmitter = this.FindClosestEmitter();
            if( parentEmitter != null )
            {
                eventChild.InformChildrenOfNewEmitter( parentEmitter.ExternalEventSource() );
            }
        }
    }

    public override void RemoveFromParent()
    {
        // first, undo EventLanguageObject things - don't react to event anymore
        if( myListeningTriggerEvent != "" )
        {
            this.InformChildrenOfNewEmitter( "" );
        }

        // undo storage - remove me from my parent's children and then forget my parent
        this.myEventParent.myEventChildren.Remove( this );
        this.myEventParent = null;

        // then, undo LanguageObject things (storage, send LosingChuck, etc)
        base.RemoveFromParent();
    }

    public override ChuckInstance GetChuck()
    {
        // only connect things to TheChuck...
        // This might need to be here so that numbers, etc. know they are
        // properly hooked up when they are properly hooked up
        return TheChuck.Instance;
    }

    private IEventLanguageObjectEmitter FindClosestEmitter()
    {
        // find the closest event emitter, starting with this and 
        // walking up the parent tree
        EventLanguageObject searchCurrent = this;
        while( searchCurrent != null )
        {
            if( searchCurrent.myMaybeEmitter != null )
            {
                return searchCurrent.myMaybeEmitter;
            }
            searchCurrent = searchCurrent.myEventParent;
        }

        return null;
    }

    private void InformChildrenOfNewEmitter( string newTriggerEvent )
    {
        // first, replace my own
        if( myMaybeListener != null )
        {
            this.ReplaceEmitter( newTriggerEvent );
        }

        // next, if I'm not an emitter myself, tell all my children
        // (propagate downward until the next emitter)
        if( myMaybeEmitter == null )
        {
            foreach( EventLanguageObject child in myEventChildren )
            {
                child.InformChildrenOfNewEmitter( newTriggerEvent );
            }
        }
    }

    private void ReplaceEmitter( string newTriggerEvent )
    {
        if( myListeningTriggerEvent != "" )
        {
            // deregister
            myMaybeListener.LosingListenEvent( TheChuck.Instance, myListeningTriggerEvent );
            TheChuck.Instance.StopListeningForChuckEvent( myListeningTriggerEvent, myTriggerCallback );
        }
        
        // register
        myListeningTriggerEvent = newTriggerEvent;
        if( myListeningTriggerEvent != "" )
        {
            TheChuck.Instance.StartListeningForChuckEvent( myListeningTriggerEvent, myTriggerCallback );
            myMaybeListener.NewListenEvent( TheChuck.Instance, newTriggerEvent );
        }
    }

    private void ListenTriggerCallback()
    {
        myTriggerCount++;
    }

}

public interface IEventLanguageObjectListener : ILanguageObjectListener
{
    void TickDoAction();
    void NewListenEvent( ChuckInstance theChuck, string incomingEvent );
    void LosingListenEvent( ChuckInstance theChuck, string losingEvent );
}

public interface IEventLanguageObjectEmitter : ILanguageObjectListener
{
    // return a string which is the Event people should listen to
    string ExternalEventSource();
    void StartEmitTrigger();
}
