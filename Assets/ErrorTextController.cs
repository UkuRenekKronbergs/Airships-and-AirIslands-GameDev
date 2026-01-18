using UnityEngine;
using TMPro;

public class ErrorTextController : MonoBehaviour
{
    TMP_Text text;

    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    public void setErrorText(string errorText)
    {
        text.text = errorText;
    }

    public void clearErrorText()
    {
        text.text = "";
    }
}
