using UnityEngine;

public class Compartment : MonoBehaviour
{
    [SerializeField, Tooltip("Categorises the compartment layout for other systems.")]
    private int compartmentType;

    public GameObject Left_Room;
    public GameObject Right_Room;

    public bool Is_Buildable = false;

    private bool _isEmpty = true;

    public int CompartmentType => compartmentType;

    public bool Is_Empty
    {
        get => _isEmpty;
        set
        {
            if (_isEmpty == value)
            {
                return;
            }

            _isEmpty = value;

            if (!_isEmpty)
            {
                Is_Buildable = false;
                UpdateNeighbourBuildableState();
            }
        }
    }

    private void UpdateNeighbourBuildableState()
    {
        TryEnableBuildable(Left_Room);
        TryEnableBuildable(Right_Room);
    }

    private static void TryEnableBuildable(GameObject neighbour)
    {
        if (neighbour == null)
        {
            return;
        }

        var neighbourCompartment = neighbour.GetComponent<Compartment>();
        if (neighbourCompartment != null && neighbourCompartment.Is_Empty)
        {
            neighbourCompartment.Is_Buildable = true;
        }
    }
}
