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
    public Dictionary<string, HashSet<GameObject>> AllCompartments = new Dictionary<string, HashSet<GameObject>>();
    //public Dictionary<string, int> MinimumAmmounts = new Dictionary<string, int>();



    //public GameObject location;



    private void Awake()
    {
        Instance = this;
        AllCompartments_func();
        // Stupid way to do this. There should probs be a masterlist somewhere of all possible compartments where you could pull this type of info.
        // Controllers and shit would help. Oh well...
        //MinimumAmmounts["Weapons"] = 1;
        //MinimumAmmounts["Bridge"] = 1;
        //MinimumAmmounts["Storage"] = 1;
        //MinimumAmmounts["Engine"] = 1;







    }
    public void AllCompartments_func() {
        AllCompartments = new Dictionary<string, HashSet<GameObject>>();// Kind of defeats the purpose of using hasshet. But I am too lazy rn to add the ability to remove items from the hashset.
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
                if (a == null)
                {
                    continue;
                }
                if (AllCompartments.ContainsKey(a.Name))
                {
                    AllCompartments[a.Name].Add(Compartment);
                }
                else
                {
                    AllCompartments.Add(a.Name, new HashSet<GameObject>());
                    AllCompartments[a.Name].Add(Compartment);
                }
            }
        }
    }


    void Start()
    {
        GPT_Debug();
    }

    void Update()
    {
        
    }


    public void GPT_Debug() {
        foreach (var kvp in AllCompartments)
        {
            string key = kvp.Key;
            HashSet<GameObject> valueSet = kvp.Value;

            

            // Safely handle MonoBehaviour keys (attached to GameObjects)
            

            Debug.Log($"Key: {key}");

            foreach (GameObject obj in valueSet)
            {
                Debug.Log($"   -> {obj.name}");
            }
            Debug.Log("Next Key");
        }



    }



}
