using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashController : MonoBehaviour {

	public void OnTriggerEnter(Collider other)
    {
        if( !GetComponent<RendererController>().beingRendered )
        {
            // do nothing if I am not visible
            return;
        }
        Transform objectToSearch = other.transform;
        while( objectToSearch )
        {
            LanguageObject lo = objectToSearch.GetComponent<LanguageObject>();
            if( lo != null && ShouldDestroy( lo ) )
            {
                // tell parent.  I think that any kids will be auto destroyed.
                if( lo.myParent != null )
                {
                    lo.RemoveFromParent();
                }
                CommentController maybeComment = lo.GetComponent<CommentController>();
                if( maybeComment )
                {
                    maybeComment.OnTrash();
                }
                Destroy( objectToSearch.gameObject );
                return;
            }
            objectToSearch = objectToSearch.parent;
        }
    }

    private bool ShouldDestroy( LanguageObject lo )
    {
        MovableController mc = lo.GetComponent<MovableController>();
        if( mc != null && !mc.amBeingMoved )
        {
            // don't delete things if they aren't being moved.
            // things that just run into the trash should not be deleted
            return false;
        }

        FunctionController fc = lo.GetComponent<FunctionController>();
        if( fc != null && fc.insideFunction )
        {
            return false;
        }

        if( lo.GetComponent<FunctionInputController>() != null || 
            lo.GetComponent<FunctionOutputController>() != null )
        {
            return false;
        }

        return true;
        
    }
}
