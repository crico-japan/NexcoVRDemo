using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderText : MonoBehaviour
{
    [SerializeField] string format = "{0}x";
    [SerializeField] TextMeshProUGUI text;

    private void Awake()
    {
        GetComponent<Slider>().onValueChanged.AddListener(SetValue);
    }

    public void SetValue(float value)
    {
        string textValue = string.Format(format, value.ToString("N1"));
        if (text.text != textValue)
            text.text = textValue;
    }
}
