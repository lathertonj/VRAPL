using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Serializer : MonoBehaviour {

    private static string myLanguageObjectSerialDir;
    private static int mySerialCounter = 0;
    
    public bool shouldLoad = false;
    public bool shouldStore = true;
    public bool shouldLoadFromResources = true;

    private void Start()
    {
        myLanguageObjectSerialDir = Application.persistentDataPath + "/serials/languageobjects";

        // make sure exists
        Directory.CreateDirectory( myLanguageObjectSerialDir );
        // deserialize everything in myLanguageObjectSerialDir
        if( shouldLoad )
        {
            foreach( string file in Directory.GetFiles( myLanguageObjectSerialDir ) )
            {
                DeserializeLanguageObject( file );
            }
        }

        if( shouldLoadFromResources )
        {
            UnityEngine.Object[] savedPrograms = Resources.LoadAll( "SavedLanguageObjects" );
            foreach( UnityEngine.Object program in savedPrograms )
            {
                TextAsset textAsset = program as TextAsset;
                Stream stream = new MemoryStream( textAsset.bytes );
                BinaryFormatter formatter = new BinaryFormatter();
                LanguageObjectSerialStorage storage = (LanguageObjectSerialStorage) formatter.Deserialize( stream );
                LanguageObject.DeserializeObject( storage );
                
            }
        }
    }

    private void OnApplicationQuit()
    {
        // don't store if flag
        if( !shouldStore )
        {
            return;
        }

        // make sure we're on the ground
        while( TheRoom.InAFunction() )
        {
            TheRoom.GetCurrentFunction().ExitFunction();
        }

        // empty myLanguageObjectSerialDir
        Directory.Delete( myLanguageObjectSerialDir, true );
        Directory.CreateDirectory( myLanguageObjectSerialDir );
        
        // reset counter 
        mySerialCounter = 0;

        // store all language objects
        GameObject[] objects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for( int i = 0; i < objects.Length; i++ )
        {
            LanguageObject maybeLanguageObject = objects[i].GetComponent<LanguageObject>();
            if( maybeLanguageObject != null && objects[i].activeInHierarchy )
            {
                Serialize( maybeLanguageObject );
            }
        }
        
    }

    public static void Serialize( LanguageObject o )
    {
        string filenameLocation = string.Format( "{0}/{1}.bytes", myLanguageObjectSerialDir, mySerialCounter );
        mySerialCounter++;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create( filenameLocation );

        LanguageObjectSerialStorage storage = o.SerializeObject();

        bf.Serialize( file, storage );
        file.Close();
    }

    public static void DeserializeLanguageObject( string filename )
    {
        if( File.Exists( filename ) )
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open( filename, FileMode.Open );
            LanguageObjectSerialStorage storage = (LanguageObjectSerialStorage) bf.Deserialize( file );
            file.Close();

            LanguageObject.DeserializeObject( storage );
        }
    }

    public static float[] SerializeVector3( Vector3 v )
    {
        return new float[] { v.x, v.y, v.z };
    }

    public static Vector3 DeserializeVector3( float[] f )
    {
        return new Vector3( f[0], f[1], f[2] );
    }

    public static float[] SerializeQuaternion( Quaternion q )
    {
        return new float[] { q.x, q.y, q.z, q.w };
    }

    public static Quaternion DeserializeQuaternion( float[] f )
    {
        return new Quaternion( f[0], f[1], f[2], f[3] );
    }
}
