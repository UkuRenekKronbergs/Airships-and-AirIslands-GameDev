using UnityEngine;

public class EmptyCompartment : CompartmentType

{
    private void Reset()
    {
        Name = "EmptyCompartment";
        MaxAmmount = 99;
        MinAmmount = 0;
        Health = 1;
        Cost = 0;
        MaxTier = 1;
        Size = 0;//will this cause problems?;
    }

    private void Awake()
    {
  
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
