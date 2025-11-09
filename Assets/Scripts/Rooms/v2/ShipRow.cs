using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class ShipRow : MonoBehaviour
{


    //[HideInInspector]

    public bool Isolated = false;
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
    private void Initialize()
    {
        List<Transform> columns = new List<Transform>();
        // Both the CombinedCompartment script and CopartmentType script are attached to the same gameobject.
        // CompartmentType is also a field of CombinedCompartment because thats still the main script.
        //GameObject Emptyroom = Instantiate(CompartmentHolder.Instance.EmptyCompartment);


        //GameObject newCombinedCompartment = new GameObject("EmptyCompartment", typeof(CombinedCompartment), typeof(EmptyCompartment));
        //GameObject newCombinedCompartment = new GameObject("EmptyCompartment", typeof(CombinedCompartment));// if add Empytycompartment as typeof it will not get the correct "reset" values.
        GameObject newCombinedCompartment = Instantiate(CompartmentHolder.Instance.EmptyCompartment);
        newCombinedCompartment.AddComponent<CombinedCompartment>();
        newCombinedCompartment.GetComponent<CombinedCompartment>().CompartmentType = newCombinedCompartment.GetComponent<EmptyCompartment>();
        newCombinedCompartment.GetComponent<CombinedCompartment>().CompartmentPrefab = CompartmentHolder.Instance.EmptyCompartment;// DO i even need to hav ea type field if I also have an instance field anyway?
        newCombinedCompartment.name = "EmptyCompartment";



        foreach (Transform child in transform)
        {
            columns.Add(child);
        }

        int size = columns.Count;
        for (int i = 0; i <size; i++)
        {

            //Leftmost
            if (i == 0)
            {
                columns[i].GetComponent<Column>().LeftColumn = null;
                if (size > 1)
                    columns[i].GetComponent<Column>().RightColumn = columns[i + 1].GetComponent<Column>();
            }
            //Rightmost
            else if (i == size - 1) {
                columns[i].GetComponent<Column>().RightColumn = null;
                // Most likely this check will never fail, because its connect with else if to if i==0
                if (size > 1)
                    columns[i].GetComponent<Column>().LeftColumn = columns[i - 1].GetComponent<Column>();
            }
            else
            {
                columns[i].GetComponent<Column>().LeftColumn = columns[i - 1].GetComponent<Column>();
                columns[i].GetComponent<Column>().RightColumn = columns[i + 1].GetComponent<Column>();
            }
            columns[i].transform.SetParent(newCombinedCompartment.transform);



        }

        newCombinedCompartment.transform.SetParent(this.transform);
        }
    



    public void RefreshValues()
    {
        RowsCombinedCompartments = new List<GameObject>();
        foreach (Transform child in transform)
        {
            RowsCombinedCompartments.Add(child.gameObject);
            if (child.GetComponent<BridgeCompartment>()!=null)
                HasBridge= true;


        }



        // TODO ElevatorCompartments list refresh, and has bridge refresh. Honestly probably shouldnt stuff all these values in one function?
    }





}
