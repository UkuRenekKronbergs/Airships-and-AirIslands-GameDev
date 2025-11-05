using UnityEngine;

public class Column : MonoBehaviour
{

    public GameObject Top;
    public GameObject Middle;
    public GameObject Bottom;
    private bool _outline = false;
    // Not TESTED!!
    public bool Outline {
        get { return _outline;}
        set {
            if (value)
            {
                Top.transform.Find("Outline").GetComponent<SpriteRenderer>().enabled = true;
                Middle.transform.Find("Outline").GetComponent<SpriteRenderer>().enabled = true;
                Bottom.transform.Find("Outline").GetComponent<SpriteRenderer>().enabled = true;
            }
            else {
                Top.transform.Find("Outline").GetComponent<SpriteRenderer>().enabled = false;
                Middle.transform.Find("Outline").GetComponent<SpriteRenderer>().enabled = false;
                Bottom.transform.Find("Outline").GetComponent<SpriteRenderer>().enabled = false;

            }


        }
    
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }




}
