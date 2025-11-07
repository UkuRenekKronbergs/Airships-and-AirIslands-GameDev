using UnityEngine;

public class EngineCompartment : CompartmentType
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void Reset()
    {
        Name = "Engine";
        MaxAmmount = 3;
        MinAmmount = 1;
        Health = 1;
        Cost = 0;
        MaxTier = 3;
        Size = 3;
    }
    private void Awake()
    {
        Name = "EngineCompartment";
        MaxAmmount = 3;
        MinAmmount = 1;
        Health = 1;
        Cost = 0;
        MaxTier = 3;
        Size = 3;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
