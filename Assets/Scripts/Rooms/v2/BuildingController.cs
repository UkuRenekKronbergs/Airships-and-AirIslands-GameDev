using UnityEngine;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour
{
    public bool BuildingModeAllowed = true;//disable button during combat
    public GameObject BuildPanel;
    public bool IsBuilding = false;
    // Add in editor
    public List<GameObject> CompartmentCards = new List<GameObject>();
    //publix 
    //public testingCompartment = 

   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleBuildMode() {
        IsBuilding = !IsBuilding;

        //Todo, finalisebuild. subdract resources, finalize.
    }
}
