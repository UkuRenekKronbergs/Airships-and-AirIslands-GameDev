using UnityEngine;

public class Compartment : MonoBehaviour

{
    //static Compartment instance;
    //public int type = 0;//add type based on child.
    //private Compartment_Type Compartment_Type = new(Compartment_Empty;


    //[HideInInspector]
    public GameObject Left_Room;
    //[HideInInspector]
    public GameObject Right_Room;

    public GameObject Compartment_Type_Prefab;
    public GameObject Child_Compartment;
    private int Current_Tier = 0;



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


    private void Awake()
    {
        Add_Compartment_Type_Child(Compartment_Type_Prefab);

        
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void Add_Compartment_Type_Child(GameObject TypePrefab) {

        if (Child_Compartment != null) {
        // TODO: return resources that are hold up it the child comaprtment if any exists.
            Destroy(Child_Compartment);
        
        }
        Current_Tier = 1;
        Child_Compartment = Instantiate(TypePrefab);
        Child_Compartment.transform.SetParent(this.transform,false);
        Child_Compartment.transform.localPosition = Vector3.zero;
        Child_Compartment.transform.localScale = Vector3.one;
       
    }


    // TODO, IMPLEMENT MERGING
    public void Check_Merge() {

        // Leftside
        if (Not_an_Elevator(Left_Room)) {
            return;
        
        
        }


    
    }

        public bool Not_an_Elevator(GameObject a) {

        if (a.GetComponent<Compartment>() != null)
            return true;
        else 
            return false;
    
    
    
    }
    public Compartment_Type ReturnType() {
        return Child_Compartment.GetComponent<Compartment_Type>();
    }






}
