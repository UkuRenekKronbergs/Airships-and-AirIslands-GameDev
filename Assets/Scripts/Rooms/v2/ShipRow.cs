using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ShipRow : MonoBehaviour
{


    //[HideInInspector]

    public bool CanBeReached = false;
    //[HideInInspector]
    public List<GameObject> RowsCombinedCompartments = new List<GameObject>();

    // Get from RowsCombinedCompartments
    public List<GameObject> ElevatorCompartments = new List<GameObject>();

    // If Bridge, can always be reached. Other rows check if they have elevator to this row to be reachable.
    //[HideInInspector]
    public bool HasBridge = false;


    private void Awake()
    {

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
        RefreshValues();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Take the columns and parent them under a EmptyCompartment object.
    /// Unlike v1 I want to leave the ammount of columns more dynamic to open up the possibilty of using different row lengths.
    /// So EmptyCompartment is generate in code instead of being assigned in the editor. With no fixed size.
    /// Also because there EmptyCompartments will now be generated and destroyed during runtime anyway and I want to avoid possible conflict between
    /// those generated and made in the editor.
    /// </summary>
    private void Initialize() { 
        List<Transform> columns = new List<Transform>();
        // Both the CombinedCompartment script and CopartmentType script are attached to the same gameobject.
        // CompartmentType is also a field of CombinedCompartment because thats still the main script.
        //GameObject Emptyroom = Instantiate(CompartmentHolder.Instance.EmptyCompartment);


        GameObject newCombinedCompartment = new GameObject("EmptyCompartment", typeof(CombinedCompartment), typeof(EmptyCompartment));
        newCombinedCompartment.GetComponent<CombinedCompartment>().CompartmentType = newCombinedCompartment.GetComponent<EmptyCompartment>();
        //newCombinedCompartment.GetComponent<CombinedCompartment>().test = CompartmentHolder.Instance.EmptyCompartment;


        foreach (Transform child in transform) { 
            columns.Add(child);
        }

        foreach (Transform child in columns)
        {
            child.transform.SetParent(newCombinedCompartment.transform);
        }
        newCombinedCompartment.transform.SetParent(this.transform);
    }



    private void RefreshValues()
    {
        RowsCombinedCompartments = new List<GameObject>();
        foreach (Transform child in transform)
        {
            RowsCombinedCompartments.Add(child.gameObject);
        }
        // TODO ElevatorCompartments list refresh, and has bridge refresh. Honestly probably shouldnt stuff all these values in one function?


    }
}
