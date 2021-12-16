using UnityEngine;
using UnityEngine.Assertions;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Transform[] cameraPositions = new Transform[] { };
    [SerializeField] Transform cameraObject = null;

    private void Awake()
    {
        Assert.IsNotNull(cameraObject);
        Assert.IsNotNull(cameraPositions);
        Assert.IsTrue(cameraPositions.Length > 0);
        foreach (Transform cameraPosition in cameraPositions)
            Assert.IsNotNull(cameraPosition);
    }

    private void Start()
    {
        SetCameraPosition(0);
    }

    public void SetCameraPosition(int index)
    {
        index = index % cameraPositions.Length;

        Transform cameraPosition = cameraPositions[index];
        cameraObject.position = cameraPosition.position;
        cameraObject.rotation = cameraPosition.rotation;
    }
}
