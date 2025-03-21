using UnityEngine;

public class ShowTextOnScreen : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Hello, world!");
    }
}
