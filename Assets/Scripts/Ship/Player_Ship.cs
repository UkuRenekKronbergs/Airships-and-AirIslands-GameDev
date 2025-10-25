using System.Collections.Generic;
using UnityEngine;

public class Player_Ship : MonoBehaviour
{
    public static Player_Ship instance;
    public int Hull = 10;
    public int Currency = 100;
    //private Dictionary<string>




    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
