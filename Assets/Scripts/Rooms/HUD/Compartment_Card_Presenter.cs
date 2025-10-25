using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Compartment_Card_Presenter : MonoBehaviour
{

    public GameObject Compartment_Prefab;
    public Texture2D Mouse_Icon_Neutral;
    public Texture2D Mouse_Icon_Positive;
    public Texture2D Mouse_Icon_Negative;
    private Vector2 mouse_hotspot;
    private Compartment_Type Compartment;
    public TextMeshProUGUI Name_Text;
    public TextMeshProUGUI Cost_Text;
    private Button _button;
    private bool _pressed = false;

    private void Awake()
    {
        Compartment = Compartment_Prefab.GetComponent<Compartment_Type>();
        Name_Text.SetText(Compartment.Name );
        Cost_Text.SetText(Compartment.Cost.ToString());


        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(Pressed);
        }
        mouse_hotspot = new Vector2(Mouse_Icon_Neutral.width / 2f, Mouse_Icon_Neutral.height / 2f);
    }


    void Start()
    {
        
    }

    void Update()
    {
        // Broke boy/girl/they
        if (Player_Ship.Instance.Currency < Compartment.Cost) {
            Cost_Text.color = Color.red;
        }





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
        if (Player_Ship.Instance.Currency >= Compartment.Cost)
        {
            Player_Ship.Instance.Currency -=Compartment.Cost;
            Compartment comp = hit.collider.GetComponent<Compartment>();
            comp.Add_Compartment_Type_Child(Compartment_Prefab);
            Player_Ship.Instance.AllCompartments_func();
            Player_Ship.Instance.GPT_Debug();

        }

    }

    //TODO
    private void Merge_Compartment(GameObject Compartment_Prefab, RaycastHit2D hit) {
        Compartment comp = hit.collider.GetComponent<Compartment>();
        




    }


}
