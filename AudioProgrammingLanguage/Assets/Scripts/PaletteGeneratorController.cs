using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteGeneratorController : MonoBehaviour {

    public Transform[] myCategories;

    private int myCurrentCategory;

	// Use this for initialization
	void Start()
    {
		myCurrentCategory = 0;
        UpdateCategoryVisualization();
	}

    public void IncrementCategory()
    {
        myCurrentCategory++;
        myCurrentCategory %= myCategories.Length;
        UpdateCategoryVisualization();
    }

    public void DecrementCategory()
    {
        myCurrentCategory += myCategories.Length - 1;
        myCurrentCategory %= myCategories.Length;
        UpdateCategoryVisualization();
    }

    private void UpdateCategoryVisualization()
    {
        for( int i = 0; i < myCategories.Length; i++ )
        {
            myCategories[i].gameObject.SetActive( myCurrentCategory == i );
        }
    }
}
