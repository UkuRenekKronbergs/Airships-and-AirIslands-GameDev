using UnityEngine;

public class HUD : MonoBehaviour
{

    public GameObject Build_Panel;
 
    void Start()
    {
        
    }

    void Update()
    {
        
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
    public void Compartment_Selected(GameObject Compartment) {
        Debug.Log("test");
    
    
    }
}
