using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenShader : MonoBehaviour {

    public static Shader hiddenShader;
    public static Shader hiddenGradientShader;
    public static Shader hiddenTextShader;
    public Shader theShader;
    public Shader gradientShader;
    public Shader textShader;

    public static Shader antiHiddenShader;
    public static Shader antiHiddenGradientShader;
    public static Shader antiHiddenTextShader;
    public Shader antiShader;
    public Shader antiGradientShader;
    public Shader antiTextShader;

	// Use this for initialization
	void Awake () {
		hiddenShader = theShader;
        hiddenGradientShader = gradientShader;
        hiddenTextShader = textShader;

        antiHiddenShader = antiShader;
        antiHiddenGradientShader = antiGradientShader;
        antiHiddenTextShader = antiTextShader;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
