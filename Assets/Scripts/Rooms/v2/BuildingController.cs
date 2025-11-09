using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BuildingController : MonoBehaviour
{
    public bool BuildingModeAllowed = true;//disable button during combat
    public GameObject BuildModeButton;
    public GameObject BuildPanel;
    public bool IsBuilding = false;
    // Add in editor
    public List<GameObject> CompartmentCardPrefabs = new List<GameObject>();
    private List<GameObject> CompartmentCards = new List<GameObject>();
    public GameObject ShadowPrefab;
    private GameObject buildingShadow;

    //cardselected
    private int _currentlySelected;
    private CompartmentType _compartmentType = null;



    private void Awake()
    {
        foreach (GameObject card in CompartmentCardPrefabs) {
            GameObject c = Instantiate(card);

            CompartmentCards.Add(c);

            c.transform.SetParent(BuildPanel.transform);
        }
        for (int i = 0; i < CompartmentCards.Count; i++)
        {
            int index = i;
            CompartmentCards[index].GetComponent<Button>().onClick.AddListener(() => CardSelected(index));
        }



    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsBuilding) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            GameObject row;
            if (Input.GetMouseButtonDown(0))
            {
                CompartmentType compartmentType = CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentType;
                (bool result, List<Column> columns) = buildingShadow.GetComponent<PlacementShadow>().GetHitColumnV2();


                //ColumnsContinuous = all columns are next to eachother, columns.Count = correct number of columns for this compartment, result = Each square of each column returned the same column.
                if (buildingShadow.GetComponent<PlacementShadow>().ColumnsContinuous(columns) && columns.Count == compartmentType.Size && result)
                {
                    if (compartmentType is ElevatorCompartment) {
                        row = columns[0].GetComponentInParent<ShipRow>().gameObject;
                        if (row.GetComponent<ShipRow>().HasBridge) { 
                        
                        
                        }










                    }





                    GameObject CombinedCompartment = CheckMerge(columns, compartmentType);
                    //Debug.Log(CombinedCompartment);
                    if (CombinedCompartment == null)
                    {
                        CombinedCompartment = Instantiate(CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentPrefab);
                        CombinedCompartment.name = compartmentType.name;// unity editor name
                        CombinedCompartment.AddComponent<CombinedCompartment>();
                        CombinedCompartment.GetComponent<CombinedCompartment>().CompartmentPrefab = CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentPrefab;
                        CombinedCompartment.GetComponent<CombinedCompartment>().CompartmentType = compartmentType;
                        CombinedCompartment.GetComponent<CombinedCompartment>().CurrentTier = 1;
                        Debug.Log(CombinedCompartment);
                    }
                    else
                        CombinedCompartment.GetComponent<CombinedCompartment>().CurrentTier++;

                    GameObject subCompartment = new GameObject("sub");
                    row = columns[0].GetComponentInParent<ShipRow>().gameObject;
                    foreach (Column column in columns)
                    {
                        column.transform.SetParent(subCompartment.transform);
                        tempcolor(column);
                    }
                    subCompartment.transform.SetParent(CombinedCompartment.transform);
                    //CombinedCompartment.transform.SetParent(row.transform);
                    AddCombinedCompartmentToRow(CombinedCompartment, row);

                }

                // Didnt click to place.
                IsBuilding = false;
                if (buildingShadow != null)
                {
                    Destroy(buildingShadow);
                }

            }// if mouseubuttondown
        }

    }

    public void ToggleBuildMode() {
        IsBuilding = !IsBuilding;
        BuildPanel.SetActive(IsBuilding);

        //Todo, finalisebuild. subdract resources, finalize.
    }

    public void CardSelected(int cardIndex) {
        Debug.Log(cardIndex);
        _currentlySelected = cardIndex;

        IsBuilding = true;
        if (buildingShadow != null)
        {
            Destroy(buildingShadow);
        }
        else {
            buildingShadow = Instantiate(ShadowPrefab);
            buildingShadow.GetComponent<PlacementShadow>().CompartmentType = CompartmentCards[cardIndex].GetComponent<CompartmentCardPresenter>().CompartmentType;
            buildingShadow.GetComponent<PlacementShadow>().ShadowSize = CompartmentCards[cardIndex].GetComponent<CompartmentCardPresenter>().CompartmentType.Size;


        }




    }

    // If merge with adjecent compartment or create a new seperate one instead
    // Checks left of leftmost column and right of rightmos column (also works if only one column exists in list)
    // if null, no merge
    // if tie, choose  left side.
    //
    public GameObject CheckMerge(List<Column> columns, CompartmentType type)
    {
        Debug.Log(type);

        // Return
        int HighestFoundTier = 0;
        GameObject returncompartment = null;
        // Do NOT RETURN
        CombinedCompartment compartment = null;


        //Debug.Log(columns[0].LeftColumn != null && columns[0].LeftColumn.GetComponentInParent<CombinedCompartment>().CompartmentType == type);
        if (columns[0].LeftColumn != null && columns[0].LeftColumn.GetComponentInParent<CombinedCompartment>().CompartmentType == type)
        {
            compartment = columns[0].LeftColumn.GetComponentInParent<CombinedCompartment>();
            Debug.Log(compartment.CurrentTier < compartment.CompartmentType.MaxTier && compartment.CurrentTier > HighestFoundTier);
            if (compartment.CurrentTier < compartment.CompartmentType.MaxTier && compartment.CurrentTier > HighestFoundTier)
            {
                HighestFoundTier = compartment.CurrentTier;
                returncompartment = compartment.gameObject;

            }

        }
        if (columns[columns.Count - 1].RightColumn != null && columns[columns.Count - 1].RightColumn.GetComponentInParent<CombinedCompartment>().CompartmentType == type)
        {
            compartment = columns[columns.Count - 1].RightColumn.GetComponentInParent<CombinedCompartment>();
            if (compartment.CurrentTier < compartment.CompartmentType.MaxTier && compartment.CurrentTier > HighestFoundTier)
            {
                HighestFoundTier = compartment.CurrentTier;
                returncompartment = compartment.gameObject;
            }
        }
        return returncompartment;
    }

    // temporary way to show visually where compartments are
    public void tempcolor(Column column)
    {
        foreach (Transform square in column.transform)
        {
            square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.blue;


        }
    }

    public void AddCombinedCompartmentToRow(GameObject newCombined, GameObject row)
    {
        newCombined.transform.SetParent(row.transform);
        row.GetComponent<ShipRow>().RefreshValues();
    }





    /// <summary>
    /// If Bridge on this floor -> always allow placement. Any free tile on floor.
    /// If not bridge floor, but already have an elevator connection to bridge floor -> allow free placement. 
    /// If not bridge floor, and not connected to bridge floor (either directly or trough another floor) . Force placement above, bellow an elevator on another floor.
    /// There is the problem with free placement, that you could build an elevator anywhere, and then attach another compartment to it, then delete the elevator. So the rule that you can only place compartments nexto existing compartments could be easily ignored...
    /// So only free placement on bridge actually.....
    /// </summary>
    /// <param name="elevatorcolumn"></param>
    public void AllowBuildElevator(Column elevatorcolumn) { 

    
    
    
    
    }
    public void BuildElevator(Column elevatorcolumn) { 

    
    
    
    }




}
