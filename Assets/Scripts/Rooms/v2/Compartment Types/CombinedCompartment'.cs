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
    public int CenterColumn; //where door.//TODO

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
        if (Columns.Count == 1 || Columns.Count > 3)
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
    public void ApplyRoomSprite_v2()
    {
        //Elevators later
        if (Columns.Count == 1 || Columns.Count > 3)
        {
            return;
        }
        List<Sprite> sprites = CompartmentPrefab.GetComponent<CompartmentType>().sprites;
        SpriteRenderer a;
        for (int i = 0; i < 3; i++)
        {

            if (i == 0)
            {
                a = Columns[i].GetComponent<Column>().Top.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[0];
                a.enabled = true;
                //a = Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                //a.sprite = sprites[0];
                //a.enabled = true;
            }
            //middle
            if (i == 1)
            {
                a = Columns[i].GetComponent<Column>().Top.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = CompartmentPrefab.GetComponent<CompartmentType>().Icon;
                a.enabled = true;

                a = Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[4];
                a.enabled = true;

                a = Columns[i].GetComponent<Column>().Bottom.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[7];
                a.enabled = true;

            }
            if (i == 2)
            {
                a = Columns[i].GetComponent<Column>().Top.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                a.sprite = sprites[2];
                a.enabled = true;
                //a = Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetComponent<SpriteRenderer>();
                //a.sprite = sprites[0];
                //a.flipX = true;
                //a.enabled = true;

            }








        }




    }
    public void OnlyIcon()
    {

        for (int i = 0; i < Columns.Count; i++)
        {
            if (Columns[i].GetComponent<Column>().Outline)
            {
                Columns[i].GetComponent<Column>().Outline = false;
            }
            var a= Columns[i].GetComponent<Column>().Middle.transform.Find("SpriteTile").GetChild(0).GetComponent<SpriteRenderer>();
            a.enabled= false;
            a = Columns[i].GetComponent<SpriteRenderer>();
            a.enabled= true;
            if (CompartmentType is BridgeCompartment)
                a.color = Color.blue;
            else if (CompartmentType is EngineCompartment)
                a.color = Color.cyan;
            else
                a.color = Color.magenta;
        }
        if (CompartmentPrefab.GetComponent<CompartmentType>().Icon != null)
        {
            var b = Columns[Columns.Count / 2];
            var c = b.GetComponent<Column>().Middle.transform.Find("SpriteTile").GetChild(0).GetComponent<SpriteRenderer>();
            c.sprite = CompartmentPrefab.GetComponent<CompartmentType>().Icon;
            c.enabled = true;
            c.transform.localScale = Vector3.one * 3f;
            c.sortingOrder = 100;
        }

    }
}
