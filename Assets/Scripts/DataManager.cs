using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public int fuel = 0;
    public int ammo = 0;
    public int food = 0;
    public int currency = 0;
    public int health = 0;


    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
