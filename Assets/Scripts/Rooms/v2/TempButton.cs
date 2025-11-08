using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
public class TempButton : MonoBehaviour
{
    public GameObject ShadowPrefab;
    private Button _button;
    public bool _isbuilding=false;
    private GameObject buildingShadow;
    public GameObject compartment;


    private void Awake()
    {



        _button = GetComponent<Button>();


        if (_button != null)
        {
            _button.onClick.AddListener(ToggleBuildMode);
        }
    }
    void Start()
    {
        compartment = CompartmentHolder.Instance.EngineCompartment;

    }

    void Update()
    {
        if (_isbuilding)
        {

            //Legacy stuff, should probs change
            // TODO: Stop rooms being generated with each click, even 
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                (bool result,List<Column> columns)  = buildingShadow.GetComponent<PlacementShadow>().GetHitColumnV2();
                if (buildingShadow.GetComponent<PlacementShadow>().ColumnsContinuous(columns) && columns.Count==compartment.GetComponent<CompartmentType>().Size&& result)
                {

                    GameObject newCompartment = Instantiate(compartment);
                    newCompartment.name = compartment.name;//display name in unity hierarchy
                    // New combinedCompartment because im not implementing tier yet. TODO tiers

                    newCompartment.AddComponent<CombinedCompartment>();
                    GameObject row = new GameObject(); // This is a inefficient way to get this value.
                    foreach (Column column in columns)
                    {
                        row = column.transform.parent.parent.gameObject;//<----stupid
                        column.transform.SetParent(newCompartment.transform);
                        tempcolor(column);
                    }
                    newCompartment.transform.SetParent(row.transform);

                }
            }

        }
    }

    public void ToggleBuildMode() { 
        _isbuilding = !_isbuilding;
        Debug.Log($"ToggleBuildMode called. IsBuilding={_isbuilding}");
        if (_isbuilding) {
            
            buildingShadow = Instantiate(ShadowPrefab);
            buildingShadow.GetComponent<PlacementShadow>().ShadowSize = 6;// Set to the size specified for that compartment type
            //Debug.Log(buildingShadow.GetComponent<PlacementShadow>().ShadowSize);
        }
        if (!_isbuilding) {
            Destroy(buildingShadow);
        
        }
    
    
    }

    // temporary way to show visually where compartments are
    public void tempcolor(Column column) {
        foreach (Transform square in column.transform) {
            square.Find("Centre").GetComponent<SpriteRenderer>().color = Color.blue;


        }


    }
}
