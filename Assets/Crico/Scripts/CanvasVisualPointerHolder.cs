using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Canvas))]
public class CanvasVisualPointerHolder : MonoBehaviour
{
    [SerializeField] RectTransform pointer;
    [SerializeField] BoxCollider boxCollider;

    float baseScale;

    private void Awake()
    {
        Assert.IsNotNull(pointer);
        Assert.IsNotNull(boxCollider);
    }

    private void Start()
    {
        Vector2 anchorPivot = new Vector2(0.5f, 0.5f);
        pointer.anchorMin = anchorPivot;
        pointer.anchorMax = anchorPivot;
        pointer.pivot = anchorPivot;

        baseScale = pointer.localScale.x;

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

        RectTransform transform = GetComponent<RectTransform>();
        Rect rect = transform.rect;

        float scale = 1f;

        float maxDistX = boxCollider.size.x / 2f;
        float absDistX = Mathf.Abs(distFromCentre.x);
        float minDistX = rect.width / 2f;
        
        if (absDistX > minDistX)
        {
            float diffX = absDistX - minDistX;
            float maxDiffX = maxDistX - minDistX;
            scale = 1f - diffX / maxDiffX;
        }

        float maxDistY = boxCollider.size.y / 2f;
        float absDistY = Mathf.Abs(distFromCentre.y);
        float minDistY = rect.height / 2f;

        if (absDistY > minDistY)
        {
            float diffY = absDistY - minDistY;
            float maxDiffY = maxDistY - minDistY;
            float scaleY = 1f - diffY / maxDiffY;

            if (scaleY < scale)
                scale = scaleY;
        }

        float pointerScale = baseScale * scale;
        pointer.localScale = new Vector3(pointerScale, pointerScale, 1f);

    }

}
