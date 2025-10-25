using System.Collections.Generic;
using UnityEngine;

public class Player_Ship : MonoBehaviour
{
    public static Player_Ship Instance;
    public int Hull = 10;
    public int Currency = 100;




    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
