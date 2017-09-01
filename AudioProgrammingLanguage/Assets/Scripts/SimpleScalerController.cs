using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NumberController))]
[RequireComponent(typeof(LanguageObject))]
public class SimpleScalerController : MonoBehaviour , ILanguageObjectListener
{

    public Collider myMinCollider;
    public Collider myMaxCollider;

    public TextMesh myMinText;
    public TextMesh myMaxText;
    public MeshRenderer[] myParentConnections;
    public MeshRenderer myMinMaxConnection;

    private NumberController myMinNumber = null;
    private NumberController myMaxNumber = null;
    private ILanguageObjectListener myParent = null;
    private IDataSource myDataSource = null;
    private Color myDefaultColor;
    private bool sendingData = false;

    private string myStorageClass;
    private string myExitEvent;


    public bool AcceptableChild( LanguageObject other, Collider mine )
    {
        // accept numbers in myMin/MaxCollider
        if( mine == myMinCollider || mine == myMaxCollider )
        {
            if( other.GetComponent<NumberController>() != null )
            {
                return true;
            }
        }
        // accept IDataSources in other colliders of mine that aren't myMin/MaxCollider
        else if( other.GetComponent(typeof(IDataSource)) != null )
        {
            return true;
        }
        return false;
    }


    public void GotChuck( ChuckInstance chuck )
    {
        myStorageClass = chuck.GetUniqueVariableName();
        myExitEvent = chuck.GetUniqueVariableName();

        chuck.RunCode(string.Format(@"
            external Event {1};
            public class {0}
            {{
                static Step @ myStep;
            }}
            Step s @=> {0}.myStep;
            0 => {0}.myStep.next;

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent ));

        if( ReadyToSendData() )
        {
            ConnectData();
        }
    }

    public void LosingChuck( ChuckInstance chuck )
    {
        if( sendingData )
        {
            DisconnectData();
        }
        chuck.BroadcastEvent( myExitEvent );
    }

    public string InputConnection()
    {
        return string.Format( "{0}.myStep", myStorageClass );
    }

    public string OutputConnection()
    {
        return InputConnection();
    }

    public void NewChild( LanguageObject child, Collider mine )
    {
        if( child.GetComponent<NumberController>() != null )
        {
            if( mine == myMinCollider )
            {
                myMinNumber = child.GetComponent<NumberController>();
                child.GetComponent<NumberController>().SetColors( myMinText.color, Color.white );
            }
            else if( mine == myMaxCollider )
            {
                myMaxNumber = child.GetComponent<NumberController>();
                child.GetComponent<NumberController>().SetColors( myMaxText.color, Color.white );
            }
        }
        IDataSource possibleDataSource = (IDataSource) child.GetComponent(typeof(IDataSource));
        if( possibleDataSource != null )
        {
            myDataSource = possibleDataSource;
            if( child.GetComponent<ControllerDataReporter>() != null )
            {
                child.GetComponent<ControllerDataReporter>().SetColors( myMinText.color, myMaxText.color );
            }
        }

        if( !sendingData && ReadyToSendData() )
        {
            ConnectData();
        }
    }

    public void ChildDisconnected( LanguageObject child )
    {
        if( child.GetComponent<NumberController>() == myMinNumber )
        {
            myMinNumber = null;
        }
        else if( child.GetComponent<NumberController>() == myMaxNumber )
        {
            myMaxNumber = null;
        }
        IDataSource possibleDataSource = (IDataSource) child.GetComponent(typeof(IDataSource));
        if( possibleDataSource == myDataSource )
        {
            myDataSource = null;
        }

        if( myDataSource == null || myMinNumber == null || myMaxNumber == null )
        {
            // reset color to default
            myMinMaxConnection.material.color = myDefaultColor;
        }

        if( sendingData && !ReadyToSendData() )
        {
            DisconnectData();
        }
    }

    public void NewParent( LanguageObject parent )
    {
        ILanguageObjectListener maybeParent = (ILanguageObjectListener) parent.GetComponent( typeof(ILanguageObjectListener) );
        if( maybeParent == null )
        {
            return;
        }
        myParent = maybeParent;
    }

    public void ParentDisconnected( LanguageObject parent )
    {
        myParent = null;
    }

    // Use this for initialization
    void Start () {
		myDefaultColor = myMinMaxConnection.material.color;
	}
	
	// Update is called once per frame
	void Update () {
        // color of the main body
		if( myDataSource != null && myMinNumber != null && myMaxNumber != null )
        {
            // data chain is complete and ready to send data or actually sending data
            float dataSourceValue = myDataSource.NormValue();
            myMinMaxConnection.material.color = myMinText.color + dataSourceValue * ( myMaxText.color - myMinText.color );
        }
        else
        {
            // something is missing
            myMinMaxConnection.material.color = myDefaultColor;
        }

        // color of the connected
        if( myParent != null )
        {
            if( myDataSource == null || myMinNumber == null || myMaxNumber == null )
            {
                // data chain not complete, but need to show we are connected to a parent properly
                foreach( MeshRenderer m in myParentConnections )
                {
                    m.material.color = Color.gray;
                }
            }
            else
            {
                // same color as body
                foreach( MeshRenderer m in myParentConnections )
                {
                    m.material.color = myMinMaxConnection.material.color;
                }
            }
            
        }
        else
        {
            // no data source and no parent
            foreach( MeshRenderer m in myParentConnections )
            {
                m.material.color = myDefaultColor;
            }
        }

        
        if( sendingData )
        {
            UpdateData();
        }
	}

    private bool ReadyToSendData()
    {
        return ( myParent != null && 
                 myDataSource != null && 
                 myMinNumber != null && 
                 myMaxNumber != null &&
                 GetChuck() != null ); 
    }

    public ChuckInstance GetChuck()
    {
        return GetComponent<LanguageObject>().GetChuck();
    }

    private void ConnectData()
    {
        GetChuck().RunCode( string.Format( "{0} => {1};", OutputConnection(), myParent.InputConnection() ) );
        sendingData = true;
    }
    
    private void DisconnectData()
    {
        GetChuck().RunCode( string.Format( "{0} =< {1};", OutputConnection(), myParent.InputConnection() ) );
        sendingData = false;
    }

    private void UpdateData()
    {
        float val = myDataSource.NormValue();
        float scaledVal = myMinNumber.GetValue() + val * ( myMaxNumber.GetValue() - myMinNumber.GetValue() );
        GetChuck().RunCode( string.Format( "{0:0.000} => {1}.next;", scaledVal, OutputConnection() ) );
    }
    
    public string VisibleName()
    {
        return "scaler";
    }

}
