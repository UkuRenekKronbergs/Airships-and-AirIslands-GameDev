using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{

    public GameObject BuildButton;
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

    }

    void Update()
    {

    }


    public void EnableBuildButton() {

        if (BuildButton.activeSelf)
        {
            BuildButton.SetActive(false);
        }
        else { 
            BuildButton.SetActive(true);
        
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
