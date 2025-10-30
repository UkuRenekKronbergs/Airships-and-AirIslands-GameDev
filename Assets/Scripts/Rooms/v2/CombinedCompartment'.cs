using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CombinedCompartment : MonoBehaviour
{
    // Turn private later, all of them?
    public CompartmentType CompartmentType;
    public GameObject test;
    public int CurrentTier = 0;
    public List<GameObject> SingleCompartment = new List<GameObject>();
    public List<GameObject> Columns = new List<GameObject>(); // len is size of compartment

    // Could I just check the list?
    public GameObject LeftMostColumn;
    public GameObject RightMostColumn;
    public int CenterColumn; //where door.

    private void Awake()
    {
        
    }

    void Start()
    {






        if (CompartmentType is EmptyCompartment)
        {
            Debug.Log("test");

        }
        else { 
        
        
        
        
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Allows us to do thing like delete 1 tier off a tier 2 or 3 room.
    public void ExpandCommpartment(List<GameObject> columns) {
        GameObject newComp = new GameObject();
        foreach (GameObject child in columns)
        {
            child.transform.SetParent(newComp.transform);
        }
        newComp.transform.SetParent(this.transform);
    }




}
