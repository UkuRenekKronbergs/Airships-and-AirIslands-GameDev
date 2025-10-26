using UnityEngine;
using TMPro;
using System;
using Airships.Ship;

public class TravelNodeController : MonoBehaviour
{
    public TMP_Text travel_text;
    public PathController pathController;
    public PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        travel_text = GameObject.FindGameObjectsWithTag("TravelText")[0].GetComponent<TMP_Text>();
        pathController = GameObject.FindGameObjectsWithTag("PathRenderController")[0].GetComponent<PathController>();
        playerController = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseEnter()
    {
        NodePair path = new NodePair();
        foreach (NodePair pair in pathController.pairs)
        {
            if ((pair.a == playerController.location && pair.b == gameObject) || (pair.a == gameObject && pair.b == playerController.location))
            {
                path = pair;
            }
        }

        if (path.distance != 0)
        {
            travel_text.text = path.distance.ToString();    
        } else
        {
            travel_text.text = "No path to destination";
        }
    }
    
    void OnMouseExit() {
        travel_text.text = "";    
    }
}
