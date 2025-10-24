using UnityEngine;

public class Elevators : MonoBehaviour
{
    //public bool Up;
    //public bool Down;

    public bool Is_Buildable = false;
    public bool Is_Empty = true;

    // Set these manually
    public GameObject Up;
    public GameObject Down;

    // Handled by code in Row.cs
    public GameObject Left_Room;
    public GameObject Right_Room;

    // False == Right
    //public bool Left;
    void Start()
    {

    }

    void Update()
    {

    }
}
