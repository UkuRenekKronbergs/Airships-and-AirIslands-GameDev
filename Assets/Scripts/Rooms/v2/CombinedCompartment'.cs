using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class CombinedCompartment : MonoBehaviour
{
    // Turn private later, all of them?
    public CompartmentType CompartmentType;
    public GameObject CompartmentPrefab;
    //public GameObject test;
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
        //Debug.Log("How many times is this triggered");
        // Needs to be updated later somehow
        foreach (Transform child in transform)
        {
            Columns.Add(child.gameObject);
        }






        if (CompartmentType is EmptyCompartment)
        {
           // Debug.Log(CompartmentType is EmptyCompartment);

        }
        else {
            Debug.Log(CompartmentPrefab);
            ApplyRoomSprite();
        }

    }


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

    public void ApplyRoomSprite() {
        //Elevators later
        if (Columns.Count == 1) {
            return;
        }
      List<Sprite> sprites = CompartmentPrefab.GetComponent<CompartmentType>().sprites;
        SpriteRenderer a = Columns[1].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
        a.sprite = sprites[0];
        a.enabled = true;





    
    
    
    
    
    
    }




}
