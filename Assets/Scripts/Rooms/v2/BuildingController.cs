using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingController : MonoBehaviour
{
    public bool BuildingModeAllowed = true;//disable button during combat
    public GameObject CurrencyCounter; // THIS SHOULDNT BE HANDLED HERE
    public GameObject BuildModeButton;
    public GameObject BuildPanel;
    public bool IsBuilding = false;
    public bool InBuildMode = false;
    // Add in editor
    public List<GameObject> CompartmentCardPrefabs = new List<GameObject>();
    private List<GameObject> CompartmentCards = new List<GameObject>();
    public GameObject ShadowPrefab;
    private GameObject buildingShadow;

    //cardselected
    private int _currentlySelected;
    private CompartmentType _compartmentType = null;

    private int _selectedLastUpdate;
    private GameObject _newbuildingshadow;




    private void Awake()
    {
        foreach (GameObject card in CompartmentCardPrefabs)
        {
            GameObject c = Instantiate(card);

            CompartmentCards.Add(c);

            c.transform.SetParent(BuildPanel.transform);
        }
        for (int i = 0; i < CompartmentCards.Count; i++)
        {
            int index = i;
            CompartmentCards[index].GetComponent<Button>().onClick.AddListener(() => CardSelected(index));
            //CompartmentCards[index].GetComponent<CompartmentCardPresenter>().UpdateButtons();
        }




    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrencyCounter.GetComponent< TextMeshProUGUI >().text = PlayerShip.Instance.Currency.ToString();
        for (int i = 0; i < CompartmentCards.Count; i++)
        {
            CompartmentCards[i].GetComponent<CompartmentCardPresenter>().UpdateButtons();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (_newbuildingshadow != null) {
            for (int i = 0; i < CompartmentCards.Count; i++)
            {
                int index = i;
                CompartmentCards[index].GetComponent<Button>().enabled = false;
            }

            IsBuilding = true;
            Destroy(buildingShadow);
            buildingShadow = _newbuildingshadow;
            _newbuildingshadow = null;
            _currentlySelected = _selectedLastUpdate;

            for (int i = 0; i < CompartmentCards.Count; i++)
            {
                int index = i;
                CompartmentCards[index].GetComponent<Button>().enabled=true;
            }
        }




        if (IsBuilding&& InBuildMode)
        {
            Debug.Log("test");
            Debug.Log(buildingShadow != null);
            //int selectedThisCycle = _currentlySelected;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            GameObject row;
            if (Input.GetMouseButtonDown(0))
            {
                
                CompartmentType compartmentType = CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentType;
                (bool result, List<Column> columns) = buildingShadow.GetComponent<PlacementShadow>().GetHitColumnV2();


                //ColumnsContinuous = all columns are next to eachother, columns.Count = correct number of columns for this compartment, result = Each square of each column returned the same column.
                //if (buildingShadow.GetComponent<PlacementShadow>().ColumnsContinuous(columns) && columns.Count == compartmentType.Size && result)
                if(buildingShadow.GetComponent<PlacementShadow>().AllowedToPlace)
                {

                    (GameObject left, GameObject right) Check = CheckMerge(columns, compartmentType);

                    GameObject CCGO;//combined compartment object
                    GameObject subCompartment = new GameObject("sub");
                    row = columns[0].GetComponentInParent<ShipRow>().gameObject;
                    foreach (Column column in columns)
                    {
                        column.transform.SetParent(subCompartment.transform);
                        tempcolor(column);
                    }
                    // no merge
                    if (Check.right == null && Check.left == null)
                    {
                        CCGO = Instantiate(CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentPrefab);
                        CCGO.name = compartmentType.name;// unity editor name
                        CCGO.AddComponent<CombinedCompartment>();
                        CCGO.GetComponent<CombinedCompartment>().CompartmentPrefab = CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentPrefab;
                        CCGO.GetComponent<CombinedCompartment>().CompartmentType = compartmentType;
                        CCGO.GetComponent<CombinedCompartment>().CurrentTier = 1;
                        subCompartment.transform.SetParent(CCGO.transform);
                        CCGO.GetComponent<CombinedCompartment>().SubCompartments.Add(subCompartment);
                        foreach (Column column in columns)
                        {
                            CCGO.GetComponent<CombinedCompartment>().Columns.Add(column.gameObject);
                        }



                        //Debug.Log(CCGO);
                    }
                    else
                    {
                        //Merge Both
                        if (Check.left != null && Check.right != null)
                        {
                            CCGO = Check.left;
                            MergeCombinedAndSub(CCGO, subCompartment, "left");
                            MergeTwoCombinedCompartments(CCGO, Check.right);
                        }
                        //merge with left
                        else if (Check.left != null)
                        {
                            CCGO = Check.left;
                            MergeCombinedAndSub(CCGO, subCompartment, "left");
                        }
                        // merge with right
                        else
                        {
                            Debug.Log("Merge with right");
                            CCGO = Check.right;
                            MergeCombinedAndSub(CCGO, subCompartment, "right");
                        }

                    }
                    //row = columns[0].GetComponentInParent<ShipRow>().gameObject;

                    //subCompartment.transform.SetParent(CCGO.transform);
                    //CCGO.GetComponent<CombinedCompartment>().SubCompartments.Add(subCompartment);
                    //CombinedCompartment.transform.SetParent(row.transform);
                    AddCombinedCompartmentToRow(CCGO, row);
                    if (compartmentType is ElevatorCompartment)// should work, not tested!
                    {
                        row.GetComponent<ShipRow>().ElevatorCompartments.Add(CCGO);
                        ConnectElevators(CCGO);
                    }
                    PlayerShip.Instance.GetAllCompartments();
                    /// THIS SHOULDNT BE HERE OR AT LEAST LIKE THIS
                    if(PlayerShip.Instance.CountTiersOrSubs(compartmentType)>compartmentType.MinAmmount)
                        PlayerShip.Instance.Currency -= compartmentType.Cost;
                    CurrencyCounter.GetComponent<TextMeshProUGUI>().text = PlayerShip.Instance.Currency.ToString();



                }

                IsBuilding = false;
                Destroy(buildingShadow);
                for (int i = 0; i < CompartmentCards.Count; i++)
                {
                    CompartmentCards[i].GetComponent<CompartmentCardPresenter>().UpdateButtons();
                }
                /*
                if (buildingShadow != null)
                   {
                    Destroy(buildingShadow);

                }*/


            }// if mouseubuttondown
        }

    }

    public void ToggleBuildMode()
    {
        InBuildMode = !InBuildMode;
        BuildPanel.SetActive(InBuildMode);

        //Todo, finalisebuild. subdract resources, finalize.
    }

    public void CardSelected(int cardIndex)
    {
        Debug.Log(cardIndex);
        _selectedLastUpdate = cardIndex;

        //IsBuilding = true;

        /*
        if (buildingShadow != null)
        {
            Destroy(buildingShadow);
        }
        */
        _newbuildingshadow = Instantiate(ShadowPrefab);
        _newbuildingshadow.GetComponent<PlacementShadow>().CompartmentType = CompartmentCards[cardIndex].GetComponent<CompartmentCardPresenter>().CompartmentType;
        _newbuildingshadow.GetComponent<PlacementShadow>().ShadowSize = CompartmentCards[cardIndex].GetComponent<CompartmentCardPresenter>().CompartmentType.Size;


        




    }

    // If merge with adjecent compartment or create a new seperate one instead
    // Checks left of leftmost column and right of rightmos column (also works if only one column exists in list)
    // if null, no merge
    // if tie, choose  left side.
    // Had to rework it so its messy, didnt merge correctly when placed inbetween two compartments before.
    public (GameObject left, GameObject right) CheckMerge(List<Column> columns, CompartmentType type)
    {
        Debug.Log(type);


        //GameObject returncompartment = null;
        // Do NOT RETURN
        CombinedCompartment compartment = null;


        bool leftside = (columns[0].LeftColumn != null && columns[0].LeftColumn.GetComponentInParent<CombinedCompartment>().CompartmentType == type);
        int leftsideTier = 0;
        GameObject leftCompartment = null;

        bool rightside = (columns[columns.Count - 1].RightColumn != null && columns[columns.Count - 1].RightColumn.GetComponentInParent<CombinedCompartment>().CompartmentType == type);
        int rightsideTier = 0;
        GameObject rightCompartment = null;



        //Debug.Log(columns[0].LeftColumn != null && columns[0].LeftColumn.GetComponentInParent<CombinedCompartment>().CompartmentType == type);
        if (leftside)
        {
            compartment = columns[0].LeftColumn.GetComponentInParent<CombinedCompartment>();
            //Debug.Log(compartment.CurrentTier < compartment.CompartmentType.MaxTier && compartment.CurrentTier > HighestFoundTier);
            if (compartment.CurrentTier < compartment.CompartmentType.MaxTier)
            {
                leftsideTier = compartment.CurrentTier;
                leftCompartment = compartment.gameObject;

            }

        }
        if (rightside)
        {
            compartment = columns[columns.Count - 1].RightColumn.GetComponentInParent<CombinedCompartment>();
            if (compartment.CurrentTier < compartment.CompartmentType.MaxTier)
            {
                rightsideTier = compartment.CurrentTier;
                rightCompartment = compartment.gameObject;
            }
        }
        if (leftside && rightside)
        {
            if (leftsideTier + rightsideTier < type.MaxTier)
            {
            }
            return (leftCompartment, rightCompartment);
        }
        if (leftsideTier > rightsideTier)
            return (leftCompartment, null);
        if (rightsideTier > leftsideTier)
            return (null, rightCompartment);
        if (rightsideTier == leftsideTier && leftsideTier != 0)//tiebreaker
            return (leftCompartment, null);
        return (null, null);





    }

    // temporary way to show visually where compartments are
    public void tempcolor(Column column)
    {
        foreach (Transform square in column.transform)
        {
            var a = square.Find("Centre").GetComponent<SpriteRenderer>();
            var type = CompartmentCards[_currentlySelected].GetComponent<CompartmentCardPresenter>().CompartmentType;
            if (type is BridgeCompartment)
                a.color = Color.blue;
            else if (type is EngineCompartment)
                a.color = Color.red;
            else
                a.color = Color.green;






        }
    }

    public void AddCombinedCompartmentToRow(GameObject newCombined, GameObject row)
    {
        newCombined.transform.SetParent(row.transform);
        row.GetComponent<ShipRow>().RefreshValues();
    }

    // Merges two combined compartments.
    GameObject MergeTwoCombinedCompartments(GameObject left, GameObject right)
    {
        CombinedCompartment CombinedLeft = left.GetComponent<CombinedCompartment>();
        CombinedCompartment CombinedRight = right.GetComponent<CombinedCompartment>();
        CombinedLeft.CurrentTier += CombinedRight.CurrentTier;//should be +1 usually 
        CombinedLeft.SubCompartments.AddRange(CombinedRight.SubCompartments);
        CombinedLeft.Columns.AddRange(CombinedRight.Columns);
        foreach (GameObject elem in CombinedLeft.SubCompartments)
        {
            elem.transform.SetParent(left.transform);
        }
        Destroy(right);
        return left;
    }

    // Left as in to the left of the new sub.
    // Right as in right of the new sub
    GameObject MergeCombinedAndSub(GameObject CombinedGameObject, GameObject sub, string side)
    {
        CombinedCompartment CC = CombinedGameObject.GetComponent<CombinedCompartment>();
        CC.CurrentTier++;
        if (side == "left")
        {
            //Debug.Log("triggered left");

            CC.SubCompartments.Add(sub);
            foreach (Transform child in sub.transform)
            {
                CC.Columns.Add(child.gameObject);
                Debug.Log(child.gameObject.name);
            }
            sub.transform.SetParent(CombinedGameObject.transform);
            return CombinedGameObject;

        }
        if (side == "right")
        {
            //Debug.Log("triggered right");
            CC.SubCompartments.Insert(0, sub);
            List<GameObject> templist = new List<GameObject>();
            foreach (Transform child in sub.transform)
            {
                templist.Add(child.gameObject);
            }
            templist.AddRange(CC.Columns);
            CC.Columns = templist;
            sub.transform.SetParent(CombinedGameObject.transform);
            sub.transform.SetAsFirstSibling();
            return CombinedGameObject;



        }
        return null;










    }
    //Connects recently built Elevators to the network.
    public void ConnectElevators(GameObject ElevatorObject)
    {
        Collider2D collider = ElevatorObject.GetComponentInChildren<Collider2D>();
        ElevatorCompartment elevator = ElevatorObject.GetComponent<ElevatorCompartment>();
        Vector2 shootposition;
        Column hitcolumn;


        //Up
        shootposition = new Vector2(
                    collider.bounds.center.x,
                    collider.bounds.max.y + 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(shootposition, Vector2.up, Mathf.Infinity, buildingShadow.GetComponent<PlacementShadow>().CollisionLayerMask);

        if (hit.collider != null)
        {
            hitcolumn = hit.collider.gameObject.GetComponent<Column>();
            if (hitcolumn.GetComponentInParent<CombinedCompartment>().CompartmentType is ElevatorCompartment)
            {
                elevator.UpColumn = hitcolumn;
                hitcolumn.GetComponentInParent<ElevatorCompartment>().DownColumn = ElevatorObject.GetComponentInChildren<Column>();

            }
        }
        //Down
        shootposition = new Vector2(
            collider.bounds.center.x,
            collider.bounds.min.y - 0.01f);
        hit = Physics2D.Raycast(shootposition, Vector2.down, Mathf.Infinity, buildingShadow.GetComponent<PlacementShadow>().CollisionLayerMask);

        if (hit.collider != null)
        {
            hitcolumn = hit.collider.gameObject.GetComponent<Column>();
            if (hitcolumn.GetComponentInParent<CombinedCompartment>().CompartmentType is ElevatorCompartment)
            {
                Debug.Log("This should not trigger");
                elevator.DownColumn = hitcolumn;
                hitcolumn.GetComponentInParent<ElevatorCompartment>().UpColumn = ElevatorObject.GetComponentInChildren<Column>();

            }
        }













    }
}
