using UnityEngine;
using UnityEngine.Assertions;

public class CanvasVisualPointerHolder : MonoBehaviour
{
    [SerializeField] RectTransform pointer;

    private void Awake()
    {
        Assert.IsNotNull(pointer);
    }

    private void Start()
    {
        Vector2 anchorPivot = new Vector2(0.5f, 0.5f);
        pointer.anchorMin = anchorPivot;
        pointer.anchorMax = anchorPivot;
        pointer.pivot = anchorPivot;

        SetPointerActive(false);
    }

    public void SetPointerActive(bool value)
    {
        if (value != pointer.gameObject.activeSelf)
            pointer.gameObject.SetActive(value);
    }

    public void SetPointerPos(Vector2 distFromCentre)
    {
        pointer.anchoredPosition = distFromCentre;
    }

}
