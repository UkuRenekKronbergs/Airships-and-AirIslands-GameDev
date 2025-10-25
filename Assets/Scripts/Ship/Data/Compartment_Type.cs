using UnityEngine;

public class Compartment_Type : MonoBehaviour

{
    // PARENT CLASS FOR ALL COMPARTMENTS
    //public virtual int Health { get; set; } = 1;
    public string Name;
    public int Max_Ammount = 1; //How many compartments of this type are allowed per one ship.
    public int Health = 1;
    public int Cost = 1;
    public int Max_Tier = 3; //How many compartments of the same type next to eachother can combine into 1 mega compartment.
    public int Size = 3; //all comparments are 3 height wise. Most comparments are 3 units length vise per 




    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
