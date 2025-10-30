using UnityEngine;

public class CompartmentHolder : MonoBehaviour
{
    public static CompartmentHolder Instance;
    public GameObject EmptyCompartment;
    public GameObject BridgeCompartment;
    public GameObject EngineCompartment;

    private void Awake()
    {
        Instance = this;
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
