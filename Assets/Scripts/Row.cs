using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Row : MonoBehaviour
{
    public int CompartmentSlots = 8;
    //public GameObject EmptyRoomPrefab;
    //public GameObject Elevator_L;
    //public GameObject Elevator_R;
    //private List<GameObject> Children = new List<GameObject>();

    private List<GameObject> Compartments= new List<GameObject>();



    private void Awake()
    {
        foreach (Transform child in transform.Find("Compartments"))
        {
            Compartments.Add(child.gameObject);

        }
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
                Compartments[i].GetComponent<Elevators>().Right_Room = Compartments[i + 1];

            }
            //elevator right
            else if (i == size - 1) {
                Compartments[i].GetComponent<Elevators>().Left_Room = Compartments[i - 1];
            }




        }



    }

        void Start()
        {
        Debug.Log(Compartments);


        }


        void Update()
        {

        }
    
}
