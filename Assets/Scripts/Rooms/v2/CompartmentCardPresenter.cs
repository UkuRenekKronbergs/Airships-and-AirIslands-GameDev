using System.Collections.Generic;
using AirshipsAndAirIslands.Events;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
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
    private Color green = new Color(0.5f, 0.6f, 0.3f);
    public GameState GameState;




    private Button _button;
    private bool _selected = false;

    private void Awake()
    {
        GameState = FindFirstObjectByType<GameState>();
        CompartmentType = CompartmentPrefab.GetComponent<CompartmentType>();//I cant be bothered to write out the whole line 9 times.
        NameText.SetText(CompartmentType.Name);
        //Debug.Log(Compartment.MaxAmmount);
        CostText.SetText("Cost: " + CompartmentType.Cost.ToString());
        Icon.sprite = CompartmentType.Icon;





        _button = GetComponent<Button>();
        if (_button != null)
        {
            //_button.onClick.AddListener(ToggleSelected);
        }
        //_mouseHotspot = new Vector2(MouseIconNeutral.width / 2f, MouseIconNeutral.height / 2f);
    }


    void Start()
    {

        
        if (SetMinMaxCurrent()) {
            _button.enabled = false;
        }
        






    }

    void Update()
    {

      
        

  
        }



    /*
    else if (MandatoryFree() || Player_Ship.Instance.Currency >= Compartment.Cost)
        CostText.color = new Color(0f, 0.39f, 0f);
    */


    public void UpdateButtons() {

        if (SetMinMaxCurrent())
        {
            _button.enabled = false;

        }
        else if (MandatoryFree())
        {
            _button.enabled = true;
            CostText.color = green;

        }
        else if (GameState.Instance.GetGold() < CompartmentType.Cost)
        {
            CostText.SetText("Cost: " + CompartmentType.Cost.ToString());
            CostText.color = Color.red;
            _button.interactable = false;
        }
        else
        {
            CostText.SetText("Cost: " + CompartmentType.Cost.ToString());
            CostText.color = green;
            _button.interactable = true;

        }




    }
 
    




    public bool SetMinMaxCurrent()
    {
        MinText.SetText("Min: " + CompartmentType.MinAmmount.ToString());
        MaxText.SetText("Max: " + CompartmentType.MaxAmmount.ToString());
        //Debug.Log(PlayerShip.Instance);
        if (PlayerShip.Instance.AllCompartments.ContainsKey(CompartmentType.Name))
        {
            //Debug.Log("always");
            int a = PlayerShip.Instance.CountTiersOrSubs(CompartmentType);
            CurrentText.SetText("Current: "+a.ToString() + "/" + CompartmentType.MaxAmmount.ToString());
            if (a == CompartmentType.MaxAmmount) {
                CostText.SetText("MAX");
                CostText.color = new Color (0.2f, 0.2f, 0.2f);//should be dark gray
                return true;


            }
            else
                return false;
        }
        else
        {
            CurrentText.SetText("Current: " + "0" + "/" + CompartmentType.MaxAmmount.ToString());
            return false;

        }
    }
    
    private bool MandatoryFree()
    {
        int CurrentAmmount = PlayerShip.Instance.CountTiersOrSubs(CompartmentType);
        if (CurrentAmmount < CompartmentType.MinAmmount){
            CostText.SetText("Cost: " + "FREE");
            return true;

        }
        //CostText.SetText("Cost: " + CompartmentType.Cost.ToString());
        return false;
        
    }





}

