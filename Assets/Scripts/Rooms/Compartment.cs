using UnityEngine;

public class Compartment : MonoBehaviour

{
    //static Compartment instance;
    //public int type = 0;//add type based on child.
    //private Compartment_Type Compartment_Type = new(Compartment_Empty;




    public bool Is_Buildable = false;
    private bool _is_empty = true;


    public bool Is_Empty
    {
        get { return _is_empty; }

        set
        {
            if (value == false)
            {
                Is_Buildable = false;

                // Check if Elevator to the right or to the left. (Elevators use a different script in this version) 
                if (Left_Room.GetComponent<Compartment>() != null)
                {
                    if (Left_Room.GetComponent<Compartment>().Is_Empty == true)
                    {
                        Left_Room.GetComponent<Compartment>().Is_Buildable = true;
                    }
                }
                if (Right_Room.GetComponent<Compartment>() != null)
                {
                    if (Right_Room.GetComponent<Compartment>().Is_Empty == true)
                    {
                        Right_Room.GetComponent<Compartment>().Is_Buildable = true;
                    }
                }
            }
        }
    }
    //[HideInInspector]
    public GameObject Left_Room;
    //[HideInInspector]
    public GameObject Right_Room;

    private void Awake()
    {


        
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }


    


 
}
