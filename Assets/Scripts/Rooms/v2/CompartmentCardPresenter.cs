using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class CompartmentCardPresenter : MonoBehaviour
{

    public GameObject CompartmentPrefab;
    public Image Icon;
    private Vector2 _mouseHotspot;
    private CompartmentType Compartment;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI MinText;
    public TextMeshProUGUI MaxText;
    public TextMeshProUGUI CurrentText;
    public GameObject ShadowPrefab;
    private GameObject buildingShadow;



    private Button _button;
    private bool _selected = false;

    private void Awake()
    {
        Compartment = CompartmentPrefab.GetComponent<CompartmentType>();//I cant be bothered to write out the whole line 9 times.
        NameText.SetText(Compartment.Name);
        //Debug.Log(Compartment.MaxAmmount);
        CostText.SetText("Cost: "+Compartment.Cost.ToString());
        Icon.sprite = Compartment.Icon;
        SetMinMaxCurrent(Compartment);





        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(ToggleSelected);
        }
        //_mouseHotspot = new Vector2(MouseIconNeutral.width / 2f, MouseIconNeutral.height / 2f);
    }


    void Start()
    {
        //SetMinMaxCurrent(Compartment);

        //MandatoryFree();
           



        
    }

    void Update()
    {
        

        /*
        // Broke boy/girl/they
        if (Player_Ship.Instance.Currency < Compartment.Cost) 
            CostText.color = Color.red;
        
        /*
        else if (MandatoryFree() || Player_Ship.Instance.Currency >= Compartment.Cost)
            CostText.color = new Color(0f, 0.39f, 0f);
        */

        if (_selected)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

         

            
            if (Input.GetMouseButtonDown(0))
            {
                (bool result, HashSet<Column> columns) = buildingShadow.GetComponent<PlacementShadow>().GetHitColumnV2();
                /*
                Debug.Log(columns.Count == Compartment.Size);
                Debug.Log(columns.Count);
                Debug.Log(Compartment.Size);
                */

                //ColumnsContinuous = all columns are next to eachother, columns.Count = correct number of columns for this compartment, result = Each square of each column returned the same column.
                if (buildingShadow.GetComponent<PlacementShadow>().ColumnsContinuous(columns) && columns.Count == Compartment.Size && result)
                    {
                    //Debug.Log("IStheproblemhere");


                        GameObject newCompartment = Instantiate(CompartmentPrefab);
                        newCompartment.name = Compartment.name;//display name in unity hierarchy
                                                               // New combinedCompartment because im not implementing tier yet. TODO tiers
                        newCompartment.AddComponent<CombinedCompartment>();
                    newCompartment.GetComponent<CombinedCompartment>().CompartmentPrefab = CompartmentPrefab;


                        //n//ewCompartment.AddComponent<CombinedCompartment>().compar;
                        GameObject row = new GameObject(); // This is a inefficient way to get this value.
                        foreach (Column column in columns)
                        {
                            row = column.transform.parent.parent.gameObject;//<----stupid
                            column.transform.SetParent(newCompartment.transform);
                            //tempcolor(column);
                        }
                        newCompartment.transform.SetParent(row.transform);

                    }
                
                ToggleSelected();
                Cursor.SetCursor(null, _mouseHotspot, CursorMode.Auto);



            }// if mouseubuttondown
        }
    }

    public void ToggleSelected() {
        _selected = !_selected;

        if (_selected) {
            buildingShadow = Instantiate(ShadowPrefab);
            buildingShadow.GetComponent<PlacementShadow>().ShadowSize = CompartmentPrefab.GetComponent < CompartmentType>().Size;
        }
        if (!_selected)
        {
            Destroy(buildingShadow);

        }


    }

    private void Build(GameObject Compartment_Prefab, RaycastHit2D hit) {
        Debug.Log("building");


        // TODO Subdtract player resources 
        if (Player_Ship.Instance.Currency >= Compartment.Cost) //        if (Player_Ship.Instance.Currency >= Compartment.Cost && !MandatoryFree())

        {
            Player_Ship.Instance.Currency -=Compartment.Cost;
            Compartment comp = hit.collider.GetComponent<Compartment>();
            comp.Add_Compartment_Type_Child(Compartment_Prefab);

            //Flags
            comp.Is_Empty = false;


            Player_Ship.Instance.AllCompartments_func();
            //SetMinMaxCurrent(Compartment);

            // Disable button. currently no way to reenable
            if (Player_Ship.Instance.AllCompartments[Compartment.Name].Count >= Compartment.MaxAmmount) {
                _button.interactable = false;
            }





            //Player_Ship.Instance.GPT_Debug();
            //foreach (var elem in Player_Ship.Instance.AllCompartments.Keys)
            //Debug.Log(elem);


        }
        /*
        else if (MandatoryFree())
        {
            Compartment comp = hit.collider.GetComponent<Compartment>();
            comp.Add_Compartment_Type_Child(Compartment_Prefab);

            //Flags
            comp.Is_Empty = false;


            Player_Ship.Instance.AllCompartments_func();
            SetMinMaxCurrent(Compartment);

            // Disable button. currently no way to reenable
            if (Player_Ship.Instance.AllCompartments[Compartment.Name].Count >= Compartment.Max_Ammount)
            {
                _button.interactable = false;
            }
        }*/

    }

    
    public void SetMinMaxCurrent(CompartmentType Compartment) {
        MinText.SetText("Min: " + Compartment.MinAmmount.ToString());
        MaxText.SetText("Max: " + Compartment.MaxAmmount.ToString());
        /*
        //Debug.Log(Player_Ship.Instance.AllCompartments.ContainsKey(Compartment.name));
        if (Player_Ship.Instance.AllCompartments.ContainsKey(Compartment.Name))
        {
            int a = Player_Ship.Instance.AllCompartments[Compartment.Name].Count;
            CurrentText.SetText("Current: "+a.ToString() + "/" + Compartment.Max_Ammount.ToString());
        }
        else
        {
            //Debug.Log("always");
            CurrentText.SetText("Current: " + "0" + "/" + Compartment.Max_Ammount.ToString());

        }
        */
    }
    /*
    private bool MandatoryFree()
    {

        if (Player_Ship.Instance.AllCompartments.TryGetValue(Compartment.Name, out HashSet<GameObject> value))
        {
            if (value.Count < Compartment.Min_Ammount){
                CostText.SetText("Cost: " + "FREE");
                return true;
                
            }
        }
        else if (Compartment.Min_Ammount > 0)
        {
            CostText.SetText("Cost: " + "FREE");
            return true;
            
        }
        CostText.SetText("Cost: " + Compartment.Cost.ToString());
        return false;
        
    }

    //TODO
    private void Merge_Compartment(GameObject Compartment_Prefab, RaycastHit2D hit) {
        Compartment comp = hit.collider.GetComponent<Compartment>();
        
    }

    */

    // temporary way to show visually where compartments are
    public void tempcolor(Column column)
    {
        foreach (Transform square in column.transform)
        {
            square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.blue;


        }


    }
}
