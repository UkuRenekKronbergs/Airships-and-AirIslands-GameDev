using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player_Ship : MonoBehaviour
{
    public static Player_Ship Instance;
    public int Hull = 10;
    public int Currency = 100;
    public GameObject[] rows = new GameObject[3]; 
    private int Max_Storage;
    private int Storage_used;
    [HideInInspector]
    public Dictionary<Compartment_Type, HashSet<GameObject>> AllCompartments = new Dictionary<Compartment_Type, HashSet<GameObject>>();




    //public GameObject location;



    private void Awake()
    {
        Instance = this;
        AllCompartments_func();





    }
    public void AllCompartments_func() {
        AllCompartments = new Dictionary<Compartment_Type, HashSet<GameObject>>();// Kind of defeats the purpose of using hasshet. But I am too lazy rn to add the ability to remove items from the hashset.
        List<GameObject> Compartments = new List<GameObject>();

        // Note: A bit different than the code in rows.cs. In rows, compartments is the parent that holds all compartments and non of the ship edges. And we have to search for that child specifically. Here I passed the compartment holder directly in unity editor.
        foreach (var compartment in rows) {

            foreach (Transform child in compartment.transform)
            {
                Compartments.Add(child.gameObject);

            }



        }

        foreach (GameObject Compartment in Compartments)
        {
            //Not a fuckign elevator
            if ((Compartment.GetComponent<Compartment>() != null))
            {
                Compartment_Type a = Compartment.GetComponent<Compartment>().ReturnType();
                if (AllCompartments.ContainsKey(a))
                {
                    AllCompartments[a].Add(Compartment);
                }
                else
                {
                    AllCompartments.Add(a, new HashSet<GameObject>());
                    AllCompartments[a].Add(Compartment);
                }
            }
        }
    }


    void Start()
    {
        //GPT_Debug();
    }

    void Update()
    {
        
    }

    public void GPT_Debug() {
        foreach (var kvp in AllCompartments)
        {
            Compartment_Type key = kvp.Key;
            HashSet<GameObject> valueSet = kvp.Value;

            string keyLabel;

            // Safely handle MonoBehaviour keys (attached to GameObjects)
            if (key != null)
            {
                if (key is MonoBehaviour mb && mb.gameObject != null)
                    keyLabel = $"{mb.gameObject.name} ({key.GetType().Name})";
                else
                    keyLabel = key.GetType().Name;
            }
            else
            {
                keyLabel = "NULL KEY";
            }

            Debug.Log($"Key: {keyLabel}");

            foreach (GameObject obj in valueSet)
            {
                Debug.Log($"   -> {obj.name}");
            }
        }



    }



}
