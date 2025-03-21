using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmpText.text = "Hello, world!";
        tmpText.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
     
    }
}
