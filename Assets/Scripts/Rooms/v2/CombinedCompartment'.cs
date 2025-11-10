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
    public List<GameObject> SubCompartments = new List<GameObject>();
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


    }


    void Update()
    {

    }

    /*
    public void ExpandCommpartment(GameObject CombinedCompartment)
    {


 
    }
    */
    



    public void ApplyRoomSprite()
    {
        //Elevators later
        if (Columns.Count == 1)
        {
            return;
        }
        List<Sprite> sprites = CompartmentPrefab.GetComponent<CompartmentType>().sprites;
        SpriteRenderer a;
        for (int i = 0; i < Columns.Count; i++)
        {

            if (i == 0)
            {
                a = Columns[i].GetComponent<Column>().Top.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[1];
                a.enabled = true;
                a = Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[0];
                a.enabled = true;
            }
            //middle
            if (i == 1)
            {
                a = Columns[i].GetComponent<Column>().Top.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[2];
                a.enabled = true;

                a = Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[5];
                a.enabled = true;

                a = Columns[i].GetComponent<Column>().Bottom.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[4];
                a.enabled = true;

            }
            if (i == 2)
            {
                a = Columns[i].GetComponent<Column>().Top.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[3];
                a.enabled = true;
                a = Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[0];
                a.flipX = true;
                a.enabled = true;

            }








        }




    }
}
