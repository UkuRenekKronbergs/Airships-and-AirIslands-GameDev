using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class CompartmentCardPresenter : MonoBehaviour
{

    public GameObject CompartmentPrefab;
    public Image Icon;
    //private Vector2 _mouseHotspot;
    public CompartmentType CompartmentType;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI MinText;
    public TextMeshProUGUI MaxText;
    public TextMeshProUGUI CurrentText;
    //public GameObject ShadowPrefab;
    //private GameObject buildingShadow;



    private Button _button;
    private bool _selected = false;

    private void Awake()
    {
        CompartmentType = CompartmentPrefab.GetComponent<CompartmentType>();//I cant be bothered to write out the whole line 9 times.
        NameText.SetText(CompartmentType.Name);
        //Debug.Log(Compartment.MaxAmmount);
        CostText.SetText("Cost: " + CompartmentType.Cost.ToString());
        Icon.sprite = CompartmentType.Icon;
        SetMinMaxCurrent(CompartmentType);





        _button = GetComponent<Button>();
        if (_button != null)
        {
            //_button.onClick.AddListener(ToggleSelected);
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

    
    }
    /*
    public void ToggleSelected()
    {
        _selected = !_selected;

        if (_selected)
        {
            buildingShadow = Instantiate(ShadowPrefab);
            buildingShadow.GetComponent<PlacementShadow>().ShadowSize = CompartmentPrefab.GetComponent<CompartmentType>().Size;
        }
        if (!_selected)
        {
            Destroy(buildingShadow);

        }


    }
    */

    private void Build(GameObject Compartment_Prefab, RaycastHit2D hit)
    {
        Debug.Log("building");


        // TODO Subdtract player resources 
        if (Player_Ship.Instance.Currency >= CompartmentType.Cost) //        if (Player_Ship.Instance.Currency >= Compartment.Cost && !MandatoryFree())

        {
            Player_Ship.Instance.Currency -= CompartmentType.Cost;
            Compartment comp = hit.collider.GetComponent<Compartment>();
            comp.Add_Compartment_Type_Child(Compartment_Prefab);

            //Flags
            comp.Is_Empty = false;


            Player_Ship.Instance.AllCompartments_func();
            //SetMinMaxCurrent(Compartment);

            // Disable button. currently no way to reenable
            if (Player_Ship.Instance.AllCompartments[CompartmentType.Name].Count >= CompartmentType.MaxAmmount)
            {
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


    public void SetMinMaxCurrent(CompartmentType Compartment)
    {
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


    // If merge with adjecent compartment or create a new seperate one instead



}

