using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TimelineUI : MonoBehaviour
{
    [SerializeField] PlayableDirector director;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] Slider slider;

    [SerializeField] UnityEvent onEnable = new UnityEvent();
    [SerializeField] UnityEvent onDisable = new UnityEvent();

    bool isPaused;

    private void Awake()
    {
        Assert.IsNotNull(director);
        Assert.IsNotNull(cameraManager);
        Assert.IsNotNull(slider);

        isPaused = true;
    }

    private void OnEnable()
    {
        onEnable.Invoke();
        director.Pause();
        slider.value = (float)director.time / (float)director.duration;
    }

    private void OnDisable()
    {
        onDisable.Invoke();
    }

    public void ToggleActive()
    {
        if (gameObject.activeSelf)
        {
            PlayAndClose();
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetTime(float normalizedTime)
    {
        double time = (double)normalizedTime * director.duration;
        director.time = time;
    }

    public void SetCameraIndex(int index)
    {
        cameraManager.SetCameraPosition(index);
        Close();
    }

    public void PauseAndClose()
    {
        isPaused = true;
        director.Pause();
        Close();
    }

    public void PlayAndClose()
    {
        isPaused = false;
        director.Play();
        Close();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            director.Play();
            isPaused = false;
        }
        else
        {
            director.Pause();
            isPaused = true;
        }
    }

    public void RestartAndClose()
    {
        director.Stop();
        director.Play();
        Close();
    }

    private void Stop()
    {
        director.Stop();
    }
}
