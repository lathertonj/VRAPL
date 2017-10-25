using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLanguageObject : LanguageObject {

    IEventLanguageObjectListener myListener = null;

    private void Awake()
    {
        base.Awake();
        myListener = (IEventLanguageObjectListener) GetComponent( typeof(IEventLanguageObjectListener) );
    }

    public void ProcessTickMessage()
    {
        // if my tick action was successful, tell my children to process a tick too
        if( myListener.TickDoAction() )
        {
            TellChildrenToProcessTickMessage();
        }
    }

    public void TellChildrenToProcessTickMessage()
    {
        foreach( LanguageObject child in myChildren )
        {
            EventLanguageObject maybeEvent = child.GetComponent<EventLanguageObject>();
            if( maybeEvent != null )
            {
                maybeEvent.ProcessTickMessage();
            }
        }
    }

    public override ChuckInstance GetChuck()
    {
        return TheChuck.Instance;
    }
}

public interface IEventLanguageObjectListener : ILanguageObjectListener
{
    // return whether to keep propagating the tick
    bool TickDoAction();
}
