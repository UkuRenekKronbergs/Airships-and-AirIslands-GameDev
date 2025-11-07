using UnityEngine;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour
{
    public bool BuildingModeAllowed = true;//disable button during combat
    public GameObject BuildModeButton;
    public GameObject BuildPanel;
    public bool IsBuilding = false;
    // Add in editor
    public List<GameObject> CompartmentCardPrefabs = new List<GameObject>();



    private void Awake()
    {
        foreach (GameObject card in CompartmentCardPrefabs) { 
            Instantiate(card,BuildPanel.transform);
        
        
        }
    }


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
        BuildPanel.SetActive(IsBuilding);

        //Todo, finalisebuild. subdract resources, finalize.
    }
}
