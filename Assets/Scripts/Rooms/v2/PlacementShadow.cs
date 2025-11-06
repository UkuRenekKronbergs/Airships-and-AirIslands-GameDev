using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Networking.PlayerConnection;
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
        // The whole Needs to be either an elevator or NextTo an existing compartment

        if (NexToValid())
        {
            foreach (GameObject column in columns)
            {
                foreach (Transform square in column.transform)
                {
                    RaycastHit2D hit = Physics2D.Raycast(square.position, Vector2.zero, Mathf.Infinity, CollisionLayerMask);
                    if (hit.collider != null) {
                        Column hitcolumn = hit.collider.gameObject.GetComponent<Column>();
                        // If empty == green
                        if (hitcolumn.transform.parent.GetComponent<CombinedCompartment>().CompartmentType is EmptyCompartment)
                            square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.green;
                        else
                            square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.red;
                    }
                    else
                        square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.red;



                }
            }

        }
        


    }
    //                    square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.red;


    // Make each square fire a ray and check if at least ONE of them hit a tile next to another compartment
    public bool NexToValid() {
        
        foreach (GameObject column in columns) {
            foreach (Transform square in column.transform) {
                RaycastHit2D hit = Physics2D.Raycast(square.position, Vector2.zero, Mathf.Infinity, CollisionLayerMask);
                if (hit.collider != null) {
                    Column hitcolumn = hit.collider.gameObject.GetComponent<Column>();
                    if (hitcolumn.LeftColumn != null)
                    {
                        if (hitcolumn.LeftColumn.transform.parent.GetComponent<CombinedCompartment>().CompartmentType is not EmptyCompartment)
                            return true;
                        
                    }
                    else if (hitcolumn.RightColumn != null)
                    {
                        if (hitcolumn.RightColumn.transform.parent.GetComponent<CombinedCompartment>().CompartmentType is not EmptyCompartment)
                            return true;
                    }
                }
            }
        }
        // RETURN FALSE, FOR TESTING RETURN TRUE
        return true;
    }

}
