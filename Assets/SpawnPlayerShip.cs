using AirshipsAndAirIslands.Events;
using UnityEngine;

public class SpawnPlayerShip : MonoBehaviour
{
    private PlayerShip ship;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ship = FindFirstObjectByType<PlayerShip>();
        ship.transform.GetChild(0).gameObject.SetActive(true);
        //ship.GetAllCompartments();


        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        ship.transform.GetChild(0).gameObject.SetActive(false);
    }
}
