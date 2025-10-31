using UnityEngine;

public class BridgeCompartment : CompartmentType
{

    private void Reset()
    {
        Name = "Bridge";
        Max_Ammount = 1;
        Min_Ammount = 1;
        Health = 1;
        Cost = 0;
        Max_Tier = 1;
        Size = 6;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
