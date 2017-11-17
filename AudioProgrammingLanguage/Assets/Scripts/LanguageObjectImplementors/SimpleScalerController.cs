using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NumberController))]
[RequireComponent(typeof(LanguageObject))]
public class SimpleScalerController : MonoBehaviour , ILanguageObjectListener
{

    public TextMesh myMinText;
    public TextMesh myMaxText;
    public MeshRenderer[] myParentConnections;
    public MeshRenderer myMinMaxConnection;

    private NumberController myMinNumber = null;
    private NumberController myMaxNumber = null;
    private ILanguageObjectListener myParent = null;
    private LanguageObject myLO = null;
    private IDataSource myDataSource = null;
    private Color myDefaultColor;
    private bool sendingData = false;

    private ChuckSubInstance myChuck;
    private string myStorageClass;
    private string myExitEvent;

    public void InitLanguageObject( ChuckSubInstance chuck )
    {
        // init object
        myLO = GetComponent<LanguageObject>();
        myDefaultColor = myMinMaxConnection.material.color;

        // init chuck
        myChuck = chuck;
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
            // be quiet
            0 => {0}.myStep.gain;

            // wait until told to exit
            {1} => now;
        ", myStorageClass, myExitEvent ));

        if( ReadyToSendData() )
        {
            ConnectData();
        }
    }

    public void CleanupLanguageObject( ChuckSubInstance chuck )
    {
        if( sendingData )
        {
            DisconnectData();
        }
        chuck.BroadcastEvent( myExitEvent );
        myChuck = null;
    }
    

    public void ParentConnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        myParent = parentListener;
    }

    public void ParentDisconnected( LanguageObject parent, ILanguageObjectListener parentListener )
    {
        myParent = null;
    }

    public bool AcceptableChild( LanguageObject other, ILanguageObjectListener otherListener )
    {
        // accept numbers if either my min or max is empty
        if( other.GetComponent<NumberController>() != null && 
            ( myMinNumber == null || myMaxNumber == null ) )
        {
            return true;
        }
        // accept data sources if my data source is empty
        else if( other.GetComponent(typeof(IDataSource)) != null &&
                 myDataSource == null )
        {
            return true;
        }

        return false;
    }

    public void ChildConnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        // Don't connect any ugens: we use the numbers and IDataSource in Update(), not in chuck
        // TODO: refactor to happen in chuck?
        if( child.GetComponent<NumberController>() != null )
        {
            // try assigning min number
            if( myMinNumber == null )
            {
                myMinNumber = child.GetComponent<NumberController>();
                // don't set color until have both
                // child.GetComponent<NumberController>().SetColors( myMinText.color, Color.white );
            }
            // assign max number, then see if they need to switch
            else if( myMaxNumber == null )
            {
                myMaxNumber = child.GetComponent<NumberController>();

                // use global height to determine which is the min and which is the max
                // can't use localPosition -- when NewChild is called, it hasn't been set to my child yet.
                if( myMaxNumber.transform.position.y < myMinNumber.transform.position.y )
                {
                    NumberController temp = myMinNumber;
                    myMinNumber = myMaxNumber;
                    myMaxNumber = temp;

                }

                // set colors
                myMinNumber.SetColors( myMinText.color, Color.white );
                myMaxNumber.SetColors( myMaxText.color, Color.white );
            }
            else
            {
                Debug.LogError( "SimpleScaler received more than two numbers, something went wrong with AcceptableChild" );
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

    public void ChildDisconnected( LanguageObject child, ILanguageObjectListener childListener )
    {
        if( child.GetComponent<NumberController>() == myMinNumber )
        {
            myMinNumber = null;
            // if there's only one number, it should always be min
            if( myMaxNumber != null )
            {
                myMinNumber = myMaxNumber;
                myMaxNumber = null;
                myMinNumber.SetColors( Color.black, Color.white );
            }
        }
        else if( child.GetComponent<NumberController>() == myMaxNumber )
        {
            myMaxNumber = null;
            // reset color of myMinNumber
            myMinNumber.SetColors( Color.black, Color.white );
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

    public void SizeChanged( float newSize )
    {
        // don't care about my size
    }

    public string InputConnection( LanguageObject whoAsking )
    {
        return OutputConnection();
    }

    public string OutputConnection()
    {
        return string.Format( "{0}.myStep", myStorageClass );
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
                 myChuck != null ); 
    }

    private void ConnectData()
    {
        // from now on, parent will connect me to it automatically.
        // Maybe make this more about setting the gain of something to 0 or 1?
        myChuck.RunCode( string.Format( "1 => {0}.gain;", OutputConnection() ) );
        sendingData = true;
    }
    
    private void DisconnectData()
    {
        myChuck.RunCode( string.Format( "0 => {0}.gain;", OutputConnection() ) );
        sendingData = false;
    }

    private void UpdateData()
    {
        float val = myDataSource.NormValue();
        float scaledVal = myMinNumber.GetValue() + val * ( myMaxNumber.GetValue() - myMinNumber.GetValue() );
        myChuck.RunCode( string.Format( "{0:0.000} => {1}.next;", scaledVal, OutputConnection() ) );
    }
    
    public string VisibleName()
    {
        return "scaler";
    }

    public void CloneYourselfFrom( LanguageObject original, LanguageObject newParent )
    {
        // nothing to copy over
    }

    // Serialization for storage on disk
    public string[] SerializeStringParams( int version )
    {
        // no string params
        return LanguageObject.noStringParams;
    }

    public int[] SerializeIntParams( int version )
    {
        // no int params
        return LanguageObject.noIntParams;
    }

    public float[] SerializeFloatParams( int version )
    {
        // no float params
        return LanguageObject.noFloatParams;
    }

    public object[] SerializeObjectParams( int version )
    {
        // no object params
        return LanguageObject.noObjectParams;
    }

    public void SerializeLoad( int version, string[] stringParams, int[] intParams, 
        float[] floatParams, object[] objectParams )
    {
        // nothing to load from params
    }
}
