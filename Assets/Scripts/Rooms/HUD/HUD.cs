using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{

    public GameObject Build_Panel;
    public TextMeshProUGUI Currency_Counter;
    public HUD Instance;
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private Button mapButton;


    private void Awake()
    {
        Instance = this;
        if (mapButton != null)
        {
            mapButton.onClick.RemoveListener(LoadMapScene);
            mapButton.onClick.AddListener(LoadMapScene);
        }
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

    public void LoadMapScene()
    {
        if (Application.CanStreamedLevelBeLoaded(mapSceneName))
        {
            SceneManager.LoadScene(mapSceneName);
        }
        else
        {
            Debug.LogWarning($"HUD could not load map scene '{mapSceneName}'. Ensure it is added to Build Settings.");
        }
    }

}
