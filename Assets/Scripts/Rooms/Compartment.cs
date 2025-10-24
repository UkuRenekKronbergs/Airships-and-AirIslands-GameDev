using UnityEngine;

public class Compartment : MonoBehaviour
{
    [SerializeField, Tooltip("Categorises the compartment layout for other systems.")]
    private int compartmentType;

    public GameObject Left_Room;
    public GameObject Right_Room;

    public int CompartmentType => compartmentType;
}
