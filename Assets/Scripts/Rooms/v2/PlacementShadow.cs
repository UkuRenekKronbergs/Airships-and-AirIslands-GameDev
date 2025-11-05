using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class PlacementShadow : MonoBehaviour
{

    List<GameObject> columns = new List<GameObject>();
    public LayerMask CollisionLayerMask;
    //private bool _
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in transform)
        {
            columns.Add(child.gameObject);
            //Debug.Log(columns.Count);
        }

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        mousePos.x += 1;//mouse at centre square for at least 3x3
        transform.position = mousePos;

        foreach (GameObject column in columns)
        {
            foreach (Transform square in column.transform)
            {
                RaycastHit2D hit = Physics2D.Raycast(square.position, Vector2.zero,Mathf.Infinity, CollisionLayerMask);
                if (hit.collider!=null)
                {
                    //Debug.Log(Physics2D.Raycast(square.position, Vector2.zero, CollisionLayerMask).name);
                    Debug.Log("HIT!");
                    square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.green;
                    //square.GetComponent<SpriteRenderer>().color = Color.green;
                }
                else {
                    //square.GetComponent<SpriteRenderer>().color = Color.red;
                    square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.red;

                    Debug.Log("MISS!");
                }




            }






        }



    }
}
