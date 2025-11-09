using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerShip : MonoBehaviour
{
    public List<GameObject> Rows;
    private List<ShipRow> RowCompartments = new List<ShipRow>();


    private void Awake()
    {
        foreach (GameObject elem in Rows) {
            RowCompartments.Add(elem.GetComponentInChildren<ShipRow>());
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
}
