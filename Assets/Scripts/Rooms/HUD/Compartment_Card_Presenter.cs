using UnityEngine;
using AirshipsAndAirIslands.Audio;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class Compartment_Card_Presenter : MonoBehaviour
{

    public GameObject Compartment_Prefab;
    public Image Icon;
    public Texture2D Mouse_Icon_Neutral;
    public Texture2D Mouse_Icon_Positive;
    public Texture2D Mouse_Icon_Negative;
    private Vector2 mouse_hotspot;
    private Compartment_Type Compartment;
    public TextMeshProUGUI Name_Text;
    public TextMeshProUGUI Cost_Text;
    public TextMeshProUGUI MinText;
    public TextMeshProUGUI MaxText;
    public TextMeshProUGUI CurrentText;
    private Button _button;
    private bool _pressed = false;

    private void Awake()
    {
        Compartment = Compartment_Prefab.GetComponent<Compartment_Type>();
        Name_Text.SetText(Compartment.Name );
        Cost_Text.SetText("Cost: "+Compartment.Cost.ToString());
        Icon.sprite = Compartment.Icon;
        //SetMinMaxCurrent(Compartment);





        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(Pressed);
            // This presenter plays its own build SFX in code; mark the button so the AudioManager won't auto-add the click SFX
            if (_button.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
            {
                _button.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>();
            }
        }
        mouse_hotspot = new Vector2(Mouse_Icon_Neutral.width / 2f, Mouse_Icon_Neutral.height / 2f);
    }


    void Start()
    {
        SetMinMaxCurrent(Compartment);

        MandatoryFree();
           



        
    }

    void Update()
    {
        


        // Broke boy/girl/they
        if (Player_Ship.Instance.Currency < Compartment.Cost) 
            Cost_Text.color = Color.red;
        
        else if (MandatoryFree() || Player_Ship.Instance.Currency >= Compartment.Cost)
            Cost_Text.color = new Color(0f, 0.39f, 0f);






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
                            Build(Compartment_Prefab, hit);
                           
                        }
                    }
                }

                _pressed = false;
                Cursor.SetCursor(null, mouse_hotspot, CursorMode.Auto);



            }// if mouseubuttondown
        }
    }

    public void Pressed() {
        _pressed = true;
        Cursor.SetCursor(Mouse_Icon_Neutral,mouse_hotspot,CursorMode.Auto);
    }

    public void CursorHover(RaycastHit2D hit) {
        if (hit.collider != null)
        {
            Compartment comp = hit.collider.GetComponent<Compartment>();

            if (comp != null)//stupid elevators with their seperate components...
            {
                if (comp.Is_Empty && comp.Is_Buildable)
                {
                    Cursor.SetCursor(Mouse_Icon_Positive, mouse_hotspot, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(Mouse_Icon_Negative, mouse_hotspot, CursorMode.Auto);

                }
            }
            else
            {
                if (hit.collider.GetComponent<Elevators>() != null)
                    Cursor.SetCursor(Mouse_Icon_Negative, mouse_hotspot, CursorMode.Auto);

            }
        }
        else
            Cursor.SetCursor(Mouse_Icon_Neutral, mouse_hotspot, CursorMode.Auto);
    }

    private void Build(GameObject Compartment_Prefab, RaycastHit2D hit) {
        Debug.Log("building");


        // TODO Subdtract player resources 
        if (Player_Ship.Instance.Currency >= Compartment.Cost && !MandatoryFree())
        {
            Player_Ship.Instance.Currency -=Compartment.Cost;
            Compartment comp = hit.collider.GetComponent<Compartment>();
            comp.Add_Compartment_Type_Child(Compartment_Prefab);

            //Flags
            comp.Is_Empty = false;


            AudioManager.Instance?.PlayBuild();
            Player_Ship.Instance.AllCompartments_func();
            SetMinMaxCurrent(Compartment);

            // Disable button. currently no way to reenable
            if (Player_Ship.Instance.AllCompartments[Compartment.Name].Count >= Compartment.Max_Ammount) {
                _button.interactable = false;
            }





            //Player_Ship.Instance.GPT_Debug();
            //foreach (var elem in Player_Ship.Instance.AllCompartments.Keys)
            //Debug.Log(elem);


        }
        else if (MandatoryFree())
        {
            Compartment comp = hit.collider.GetComponent<Compartment>();
            comp.Add_Compartment_Type_Child(Compartment_Prefab);

            //Flags
            comp.Is_Empty = false;


            AudioManager.Instance?.PlayBuild();
            Player_Ship.Instance.AllCompartments_func();
            SetMinMaxCurrent(Compartment);

            // Disable button. currently no way to reenable
            if (Player_Ship.Instance.AllCompartments[Compartment.Name].Count >= Compartment.Max_Ammount)
            {
                _button.interactable = false;
            }
        }

    }


    public void SetMinMaxCurrent(Compartment_Type Compartment) {
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
                Cost_Text.SetText("Cost: " + "FREE");
                return true;
                
            }
        }
        else if (Compartment.Min_Ammount > 0)
        {
            Cost_Text.SetText("Cost: " + "FREE");
            return true;
            
        }
        Cost_Text.SetText("Cost: " + Compartment.Cost.ToString());
        return false;
        
    }

    //TODO
    private void Merge_Compartment(GameObject Compartment_Prefab, RaycastHit2D hit) {
        Compartment comp = hit.collider.GetComponent<Compartment>();
        
    }


}
