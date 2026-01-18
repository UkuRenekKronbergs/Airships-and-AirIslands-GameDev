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
            text.text += $"{type}: {GameState.Instance.GetResource(type)}\n";
        }
    }
}
