using UnityEngine;
using System.Collections.Generic;

public class CompartmentType : MonoBehaviour

{
    // PARENT CLASS FOR ALL COMPARTMENTS
    //public virtual int Health { get; set; } = 1;
    public string Name;
    //public Sprite Icon;
    public int Max_Ammount = 1; //How many compartments of this type are allowed per one ship.
    public int Min_Ammount = 0;
    public int Health = 1;
    public int Cost = 1;
    public int Max_Tier = 3; //How many compartments of the same type next to eachother can combine into 1 mega compartment.
    //public int Size = 3; //all comparments are 3 height wise. Most comparments are 3 units length vise per

    public Sprite Icon;
    public int Size = 3;
    public List<GameObject> prefabs = new List<GameObject>();






    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
