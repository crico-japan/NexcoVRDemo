using UnityEngine;
using UnityEngine.Playables;

public class MainScene : MonoBehaviour
{
    [SerializeField] GameObject ui;
    [SerializeField] float timeScale = 1f;

    private void Start()
    {
        Time.timeScale = timeScale;
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            ui.SetActive(true);
        
    }
}
