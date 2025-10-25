using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Compartment_Card_Presenter : MonoBehaviour
{

    public GameObject Compartment_Prefab;
    public Texture2D Mouse_Icon_Neutral;
    public Texture2D Mouse_Icon_Positive;
    public Texture2D Mouse_Icon_Negative;
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

    }


    void Start()
    {
        
    }

    void Update()
    {
        if (_pressed)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                Compartment comp = hit.collider.GetComponent<Compartment>();
                if (comp.Is_Empty && comp.Is_Buildable)
                {
                    Cursor.SetCursor(Mouse_Icon_Positive, Vector2.zero, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(Mouse_Icon_Negative, Vector2.zero, CursorMode.Auto);

                }
            }
            else
            {
                Cursor.SetCursor(Mouse_Icon_Neutral, Vector2.zero, CursorMode.Auto);
            }






            if (Input.GetMouseButtonDown(0))
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            }

        }
    }

    public void Pressed() {
        _pressed = true;
        Cursor.SetCursor(Mouse_Icon_Neutral,Vector2.zero,CursorMode.Auto);
    }



}
