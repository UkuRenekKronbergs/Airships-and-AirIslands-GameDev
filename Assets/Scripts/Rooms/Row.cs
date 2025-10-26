using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Row : MonoBehaviour
{
    [HideInInspector]
    public int CompartmentSlots = 8;
    //public GameObject EmptyRoomPrefab;
    //public GameObject Elevator_L;
    //public GameObject Elevator_R;
    //private List<GameObject> Children = new List<GameObject>();

    private List<GameObject> Compartments= new List<GameObject>();



    private void Awake()
    {
        // attached the children in the Unity editor. Just need to find them now.
        foreach (Transform child in transform.Find("Compartments"))
        {
            Compartments.Add(child.gameObject);

        }

        // This is  just initializing the compartments by adding the left, right, isbuildable, isempty variables.
        int size = Compartments.Count;
        Compartment comp;
        for (int i = 0; i < size; i++) {

            if (i > 0 && i != size - 1)
            {
                comp = Compartments[i].GetComponent<Compartment>();
                comp.Left_Room = Compartments[i - 1];
                comp.Right_Room = Compartments[i + 1];


            }
            //elevator left
            else if (i == 0)
            {
                //Set elevator Compartment left
                Compartments[i].GetComponent<Elevators>().Right_Room = Compartments[i + 1];
                // Set the compartment left to buildable.
                Compartments[i + 1].GetComponent<Compartment>().Is_Buildable = true;

                // In this version Is.Empty is always false for elevators.
                Compartments[i].GetComponent<Elevators>().Is_Empty = false;
                // Gonna use this together with Is_Empty to check if stuff is buildable.
                Compartments[i].GetComponent<Elevators>().Is_Buildable = true;
            }
            //elevator right
            else if (i == size - 1) {
                Compartments[i].GetComponent<Elevators>().Left_Room = Compartments[i - 1];
                Compartments[i].GetComponent<Elevators>().Is_Empty = false;
                Compartments[i].GetComponent<Elevators>().Is_Buildable = true;

                // Set the compartment right to buildable.
                Compartments[i-1].GetComponent<Compartment>().Is_Buildable = true;
            }




        }



    }

        void Start()
        {
        //Debug.Log(Compartments);


        }


        void Update()
        {

        }

    
}
