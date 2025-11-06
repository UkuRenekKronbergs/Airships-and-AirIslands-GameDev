using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class CompartmentCardPresenterOld : MonoBehaviour
{

    public GameObject CompartmentPrefab;
    public Image Icon;
    public Texture2D MouseIconNeutral;
    public Texture2D MouseIconPositive;
    public Texture2D MouseIconNegative;
    private Vector2 _mouseHotspot;
    private CompartmentType Compartment;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI MinText;
    public TextMeshProUGUI MaxText;
    public TextMeshProUGUI CurrentText;
    private Button _button;
    private bool _pressed = false;

    private void Awake()
    {
        Compartment = CompartmentPrefab.GetComponent<CompartmentType>();
        NameText.SetText(Compartment.Name );
        CostText.SetText("Cost: "+Compartment.Cost.ToString());
        Icon.sprite = Compartment.Icon;
        //SetMinMaxCurrent(Compartment);





        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(Pressed);
        }
        _mouseHotspot = new Vector2(MouseIconNeutral.width / 2f, MouseIconNeutral.height / 2f);
    }


    void Start()
    {
        //SetMinMaxCurrent(Compartment);

        //MandatoryFree();
           



        
    }

    void Update()
    {
        


        // Broke boy/girl/they
        if (Player_Ship.Instance.Currency < Compartment.Cost) 
            CostText.color = Color.red;
        
        /*
        else if (MandatoryFree() || Player_Ship.Instance.Currency >= Compartment.Cost)
            CostText.color = new Color(0f, 0.39f, 0f);
        */





        if (_pressed)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            CursorHover(hit);


            if (Input.GetMouseButtonDown(0))
            {
                if (hit.collider != null)
                {
                    Compartment comp = hit.collider.GetComponent<Compartment>();
                    if (comp != null) {
                        if (comp.Is_Empty && comp.Is_Buildable) {
                            Build(CompartmentPrefab, hit);
                           
                        }
                    }
                }

                _pressed = false;
                Cursor.SetCursor(null, _mouseHotspot, CursorMode.Auto);



            }// if mouseubuttondown
        }
    }

    public void Pressed() {
        _pressed = true;
        Cursor.SetCursor(MouseIconNeutral,_mouseHotspot,CursorMode.Auto);
    }

    public void CursorHover(RaycastHit2D hit) {
        if (hit.collider != null)
        {
            Compartment comp = hit.collider.GetComponent<Compartment>();

            if (comp != null)//stupid elevators with their seperate components...
            {
                if (comp.Is_Empty && comp.Is_Buildable)
                {
                    Cursor.SetCursor(MouseIconPositive, _mouseHotspot, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(MouseIconNegative, _mouseHotspot, CursorMode.Auto);

                }
            }
            else
            {
                if (hit.collider.GetComponent<Elevators>() != null)
                    Cursor.SetCursor(MouseIconNegative, _mouseHotspot, CursorMode.Auto);

            }
        }
        else
            Cursor.SetCursor(MouseIconNeutral, _mouseHotspot, CursorMode.Auto);
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
            if (Player_Ship.Instance.AllCompartments[Compartment.Name].Count >= Compartment.Max_Ammount) {
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

    /*
    public void SetMinMaxCurrent(CompartmentType Compartment) {
        MinText.SetText("Min: " + Compartment.Min_Ammount.ToString());
        MaxText.SetText("Max: " + Compartment.Max_Ammount.ToString());
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
    }

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
}
