using UnityEngine;

public class BridgeCompartment : CompartmentType
{

    private void Reset()
    {
        Name = "Bridge";
        MaxAmmount = 1;
        MinAmmount = 1;
        Health = 1;
        Cost = 0;
        MaxTier = 1;
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
