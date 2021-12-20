using UnityEngine;
using UnityEngine.Playables;

public class MainScene : MonoBehaviour
{
    [SerializeField] GameObject ui;
    [SerializeField] float timeScale = 1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            ui.SetActive(true);

        if (timeScale != Time.timeScale)
            Time.timeScale = timeScale;
        
    }

    public void SetTimeScale(float value)
    {
        timeScale = value;
    }
}
