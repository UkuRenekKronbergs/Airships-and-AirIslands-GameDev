using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Row : MonoBehaviour
{
    public int CompartmentSlots = 8;
    //public GameObject EmptyRoomPrefab;
    //public GameObject Elevator_L;
    //public GameObject Elevator_R;
    private List<GameObject> Children = new List<GameObject>();



    private void Awake()
    {
        foreach (Transform child in transform)
        {
            Children.Add(child.gameObject);


        }

        void Start()
        {
            foreach (GameObject child in Children)
            {
                Debug.Log(child);

            }

        }


        void Update()
        {

        }
    }
}
