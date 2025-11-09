using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.UIElements;
public class PlacementShadow : MonoBehaviour
{
    private int _shadowSize = 3;
    public int ShadowSize
    {
        get { return _shadowSize; }
        set {
            SetShadowSize(value);
            _shadowSize = value;
        }
    }


    public int size = 3; //3 == 3x3, 2=2x3, 1=1x3 etc
    List<GameObject> columns = new List<GameObject>();
    public LayerMask CollisionLayerMask;
    //public HashSet<Columns> SelectedColumns = new HashSet<Columns>;
    //private bool _

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            columns.Add(child.gameObject);
            //Debug.Log(columns.Count);
        }
    }
    void Start()
    {
  

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
                    if (hit.collider != null)
                    {
                        Column hitcolumn = hit.collider.gameObject.GetComponent<Column>();
                        // If empty == green
                        if (hitcolumn.GetComponentInParent<CombinedCompartment>().CompartmentType is EmptyCompartment)
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
    public bool NexToValid()
    {

        foreach (GameObject column in columns)
        {
            foreach (Transform square in column.transform)
            {
                RaycastHit2D hit = Physics2D.Raycast(square.position, Vector2.zero, Mathf.Infinity, CollisionLayerMask);
                if (hit.collider != null)
                {
                    Column hitcolumn = hit.collider.gameObject.GetComponent<Column>();
                    //Debug.Log(hit.collider);
                    if (hitcolumn.LeftColumn != null)
                    {
                        //Debug.Log(hitcolumn.LeftColumn.GetComponentInParent<CombinedCompartment>());
                        if (hitcolumn.LeftColumn.GetComponentInParent<CombinedCompartment>().CompartmentType is not EmptyCompartment)
                            return true;

                    }
                    else if (hitcolumn.RightColumn != null)
                    {
                        if (hitcolumn.RightColumn.GetComponentInParent<CombinedCompartment>().CompartmentType is not EmptyCompartment)
                            return true;
                    }
                }
            }
        }
        // RETURN FALSE, FOR TESTING RETURN TRUE
        return true;
    }



    // Verify that selected columns are continuous
    // Need to add verification for situation where the hashset has less columns than required for the build.
    public bool ColumnsContinuous(List<Column> columns) {
        //Same row
        List<Column> chain = new List<Column>();

        //TODO, ELEVATORS
        if (columns.Count == 1) {
            return true;
        }


        int strikes = 0;
        foreach (Column colum in columns) {

            if (colum.LeftColumn != null)
            {
                if (!columns.Contains(colum.LeftColumn))
                    strikes += 1;
            }
            if (colum.RightColumn != null)
            {
                if (!columns.Contains(colum.RightColumn))
                    strikes += 1;
            }
            // A disconected column from others.
            if (strikes == 2)
            {
                return false;
            }
            else
                strikes = 0;
        }
        // All columns are next to eachother.
        return true;
    }
    public (bool allowed, List<Column> columns) GetHitColumnV2()
    {
        List<Column> result = new List<Column>();
        int columnindex = 0;
        //Column[,] CompGrid = new Column[transform.childCount, 3];





        Column[][] CompGrid = new Column[ShadowSize][];
        for (int i = 0;i< CompGrid.Length; i++)
        {
            CompGrid[i] = new Column[3];
        }




    
        for(int i = 0; i<ShadowSize;i++)
            {
            GameObject column = columns[i];
                // Column ammount might vary, but column size is always 3

                int rowindex = 0;
                foreach (Transform square in column.transform)
                {
                    RaycastHit2D hit = Physics2D.Raycast(square.position, Vector2.zero, Mathf.Infinity, CollisionLayerMask);
                if (hit.collider != null)
                {
                    Column hitcolumn = hit.collider.gameObject.GetComponent<Column>();
                    if (hitcolumn.GetComponentInParent<CombinedCompartment>().CompartmentType is EmptyCompartment)
                    {
                        AddUnique(result, hitcolumn);
                        //result.Add(hitcolumn);
                        CompGrid[columnindex][rowindex] = hitcolumn;
                    }
                }
              
                    rowindex++;
                }
                columnindex++;
            }
            
        return (ComparisonGrid(CompGrid),result);
    }
       
    // Checking that all column squares fired for the same column. This stops compartments being built in situations where only one row of the columns is green. With this they all need to be.
    public bool ComparisonGrid(Column[][] grid)
    {

        if (grid == null || grid.Length == 0)
        {
            
            return false;
            
        }



            for (int i = 0; i < grid.Length; i++){
            Column first = grid[i][0];
            //Debug.Log(grid[i][0]);
            for (int j = 0; j < grid[i].Length; j++)
            {
                if (first != grid[i][j])
                {
                    
                    return false;
                }
               
            }
        }
        return true;
    }

    private void SetShadowSize(int s) {
       

        foreach (GameObject elem in columns) {
        elem.SetActive(true);
        }
        for (int i = s; i < columns.Count; i++) {
            columns[i].SetActive(false);
            
        }
    }

    private void AddUnique(List<Column> Columns, Column Column)
    {
        if (!Columns.Contains(Column)) { 
            Columns.Add(Column);
        }
    }

    }



