using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RendererController : MonoBehaviour {
    public static List<RendererController> allLanguageRenderers = new List<RendererController>();
    public static bool renderersCurrentlyRendering = true;

    private List<Renderer> myAutoRenderers;
    private List<Shader> myAutoShaders;
    private List<bool> myRenderersIsText;
    private List<bool> myRenderersIsGradient;
    private List<bool> myRenderersIsOutline;

    public bool beingRendered = true;

    private void Start()
    {
        Restart();
    }

    public void Restart()
    {
        allLanguageRenderers.Add(this);
        myAutoRenderers = new List<Renderer>();
        myAutoShaders = new List<Shader>();
        myRenderersIsText = new List<bool>();
        myRenderersIsGradient = new List<bool>();
        myRenderersIsOutline = new List<bool>();
        FindAllRenderers( transform );
        DeportalizeRenderers();
    }

    private void FindAllRenderers( Transform self )
    {
        Renderer r = self.GetComponent<Renderer>();
        if( r != null )
        {
            myAutoRenderers.Add( r );
            myAutoShaders.Add( r.material.shader );
            TextMesh maybeText = r.GetComponent<TextMesh>();
            myRenderersIsText.Add( maybeText != null );
            myRenderersIsGradient.Add( r.material.HasProperty( "_TopColor" ) );
            myRenderersIsOutline.Add( r.material.HasProperty( "_OutlineColor" ) );
        }

        foreach( Transform child in self )
        {
            // only search through objects that are not rendererControllers themself
            RendererController maybeRendererController = child.GetComponent<RendererController>();
            if( maybeRendererController == null )
            {
                FindAllRenderers( child );
            }
        }
    }

	public void EnableRenderers()
    {
        foreach( Renderer r in myAutoRenderers )
        {
            r.enabled = true;
        }
        beingRendered = true;
    }

    public void DisableRenderers()
    {
        foreach( Renderer r in myAutoRenderers )
        {
            r.enabled = false;
        }
        beingRendered = false;
    }

    public void PortalizeRenderers()
    {
        TryDangerousPortalChange( DangerousPortalizeRenderers );
    }

    public void DeportalizeRenderers()
    {
        TryDangerousPortalChange( DangerousDeportalizeRenderers );
    }

    private void TryDangerousPortalChange( Action PortalAction )
    {
        try
        {
            PortalAction();
        }
        catch( System.NullReferenceException e )
        {
            // something went wrong. what was it? who knows?
            myAutoRenderers = new List<Renderer>();
            myAutoShaders = new List<Shader>();
            myRenderersIsText = new List<bool>();
            myRenderersIsGradient = new List<bool>();
            myRenderersIsOutline = new List<bool>();
            FindAllRenderers( transform );

            // try again
            PortalAction();
        }
    }

    private void DangerousDeportalizeRenderers()
    {
        for( int i = 0; i < myAutoRenderers.Count; i++ )
        {
            /*
            // reset shader
            myAutoRenderers[i].material.shader = myAutoShaders[i];

            // -1 means use the value from the shader
            myAutoRenderers[i].material.renderQueue = -1;

            // cast shadows
            myAutoRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            */
            
            
            // use anti-hidden shader. text or gradient or default.
            if( myRenderersIsText[i] )
            {
                myAutoRenderers[i].material.shader = HiddenShader.antiHiddenTextShader;
            }
            else if( myRenderersIsGradient[i] )
            {
                myAutoRenderers[i].material.shader = HiddenShader.antiHiddenGradientShader;
            }
            else if( myRenderersIsOutline[i] )
            {
                // don't change shader
                // DO change whether active (default to no)
                myAutoRenderers[i].enabled = false;
            }
            else
            {
                myAutoRenderers[i].material.shader = HiddenShader.antiHiddenShader;
            }

            // set queue correctly - 4000, or 5000 for text so it is always rendered on top.
            if( myRenderersIsText[i] )
            {
                myAutoRenderers[i].material.renderQueue = 5000;
            }
            else if( !myRenderersIsOutline[i] )
            {
                myAutoRenderers[i].material.renderQueue = 4000;
            }


            // cast shadows
            myAutoRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        beingRendered = true;
    }

    private void DangerousPortalizeRenderers()
    {
        for( int i = 0; i < myAutoRenderers.Count; i++ )
        {
            // use hidden shader. text or gradient or default.
            if( myRenderersIsText[i] )
            {
                myAutoRenderers[i].material.shader = HiddenShader.hiddenTextShader;
            }
            else if( myRenderersIsGradient[i] )
            {
                myAutoRenderers[i].material.shader = HiddenShader.hiddenGradientShader;
            }
            else if( myRenderersIsOutline[i] )
            {
                // don't change shader
                // DO change whether active
                myAutoRenderers[i].enabled = false;
            }
            else
            {
                myAutoRenderers[i].material.shader = HiddenShader.hiddenShader;
            }

            // set queue correctly - 4000, or 5000 for text so it is always rendered on top.
            if( myRenderersIsText[i] )
            {
                myAutoRenderers[i].material.renderQueue = 5000;
            }
            else if( !myRenderersIsOutline[i] )
            {
                myAutoRenderers[i].material.renderQueue = 4000;
            }

            // don't cast shadows
            myAutoRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        // yes it is technically being rendered but this variable is used
        // to prevent it from being interacted with
        beingRendered = false;
    }

    public void SetOutlineEnabled( bool enabled )
    {
        for( int i = 0; i < myAutoRenderers.Count; i++ )
        {
            if( myRenderersIsOutline[i] )
            {
                // don't change shader
                // DO change whether active
                myAutoRenderers[i].enabled = enabled;
            }
        }
        
    }

    public static void TurnOn()
    {
        foreach( RendererController r in RendererController.allLanguageRenderers )
        {
            // old: completely turn on the component
            // r.EnableRenderers();
            // new: become completely visible
            r.DeportalizeRenderers();
        }
        renderersCurrentlyRendering = true;
    }

    public static void TurnOff()
    {
        foreach( RendererController r in RendererController.allLanguageRenderers )
        {
            // old: completely turn off the component
            // r.DisableRenderers();
            // new: become visible only through portal
            r.PortalizeRenderers();
        }
        renderersCurrentlyRendering = false;
    }

    public void OnDestroy()
    {
        allLanguageRenderers.Remove( this );
    }
}
