using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasTest : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData e)
    {
        Debug.Log("Pointer Down! " + e.pressPosition);
    }
}
