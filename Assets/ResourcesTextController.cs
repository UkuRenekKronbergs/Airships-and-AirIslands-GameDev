using UnityEngine;
using AirshipsAndAirIslands.Events;
using TMPro;
using System;

public class ResourcesTextController : MonoBehaviour
{
    TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "";
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            string resourceName;
            switch (type)
            {
                case ResourceType.Ammo: 
                    resourceName = "Ammo";
                    break;
                case ResourceType.Fuel: 
                    resourceName = "Fuel";
                    break;
                case ResourceType.Food:
                    resourceName = "Food";
                    break;
                case ResourceType.Gold:
                    resourceName = "Gold";
                    break;
                case ResourceType.Hull:
                    resourceName = "Hull";
                    break;
                case ResourceType.CrewMorale:
                    resourceName = "Crew Morale";
                    break;
                case ResourceType.CrewFatigue:
                    resourceName = "Crew Fatigue";
                    break;
                default: 
                    resourceName = $"{type}";
                    break;
            }

            text.text += $"{resourceName}: {GameState.Instance.GetResource(type)}\n";
        }
    }
}
