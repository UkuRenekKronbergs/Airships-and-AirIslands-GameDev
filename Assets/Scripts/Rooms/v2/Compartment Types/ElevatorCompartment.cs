using UnityEngine;

public class ElevatorCompartment : CompartmentType
{

    public Column UpColumn = null;
    public Column DownColumn = null;


    private void Reset()
    {
        Name = "Elevator";
        MaxAmmount = 99;
        MinAmmount = 0;
        Health = 1;
        Cost = 0;
        MaxTier = 1;
        Size = 1;
    }

    void Start()
    {
        
    }

 
    void Update()
    {
        
    }
}
