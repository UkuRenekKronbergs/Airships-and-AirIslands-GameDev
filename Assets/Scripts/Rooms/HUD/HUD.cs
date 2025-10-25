using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{

    public GameObject Build_Panel;
    public TextMeshProUGUI Currency_Counter;
    public HUD Instance;


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Currency_Counter.text = "Currency: "+ Player_Ship.Instance.Currency.ToString();
        //Debug.Log(Player_Ship.Instance.Currency.ToString());
    }

    void Update()
    {
        Currency_Counter.text = "Currency: " + Player_Ship.Instance.Currency.ToString();

    }

    public void Toggle_Build_Panel()
    {
        // If active, set inactive
        if (Build_Panel.activeSelf)
        {
            Build_Panel.SetActive(false);
        }
        else { 
            Build_Panel.SetActive(true);
        
        
        }

    }

}
