using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShip : MonoBehaviour
{
    public List<GameObject> Rows;
    private List<ShipRow> RowCompartments = new List<ShipRow>();
    public Dictionary<string, List<CombinedCompartment>> AllCompartments = new Dictionary<string, List<CombinedCompartment>>();

    public static PlayerShip Instance;
    //public int Hull = 0;
    //public int Currency = 0;


    private void Awake()
    {

        if (Instance == this)
        {
            // already the singleton
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        foreach (GameObject elem in Rows)
        {
            RowCompartments.Add(elem.GetComponentInChildren<ShipRow>());
        }
        //transform.GetChild(0).gameObject.SetActive(false);
        //transform.position = Vector3.zero;



    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GetAllCompartments();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetAllCompartments() {
        AllCompartments = new Dictionary<string, List<CombinedCompartment>>();
        foreach(ShipRow row in RowCompartments)
        {
            foreach(GameObject elem in row.RowsCombinedCompartments) {
                string key = elem.GetComponent<CombinedCompartment>().CompartmentType.Name;
                if (AllCompartments.TryGetValue(key, out List<CombinedCompartment> value))
                {
                    AllCompartments[key].Add(elem.GetComponent<CombinedCompartment>());

                }
                else { 
                    AllCompartments[key] = new List<CombinedCompartment>();
                    AllCompartments[key].Add(elem.GetComponent<CombinedCompartment>());
                }
            }
        }
        foreach(var key in AllCompartments.Keys)
            Debug.Log(key);
    }

    public int CountCombinedCompartments(CompartmentType type) {
        string key = type.Name;
        if (AllCompartments.TryGetValue(key, out List<CombinedCompartment> value)) {
            return AllCompartments[key].Count;
        }
        else
            return 0;
    }

    //tiers and subs should be 1:1
    public int CountTiersOrSubs(CompartmentType type) {
        string key = type.Name;
        int count = 0;
        if (AllCompartments.TryGetValue(key, out List<CombinedCompartment> value))
        {
            
            foreach(CombinedCompartment elem in AllCompartments[key])
            {
                count += elem.CurrentTier;

            }
        }
        return count;
    }
}
