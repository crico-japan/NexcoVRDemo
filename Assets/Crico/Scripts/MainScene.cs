using UnityEngine;

public class MainScene : MonoBehaviour
{
    [SerializeField] GameObject ui;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            ui.SetActive(true);
        
    }
}
