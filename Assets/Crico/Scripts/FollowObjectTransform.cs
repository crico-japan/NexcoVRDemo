using UnityEngine;
using UnityEngine.Assertions;


public class FollowObjectTransform : MonoBehaviour
{
    [SerializeField] Vector3 offset = Vector3.zero;
    
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float rotateStartAngle = 80f;
    [SerializeField] float rotateEndAngle = 10f;

    [SerializeField] Transform objectToFollowPos = null;
    [SerializeField] Transform objectToFollowRotate = null;

    bool rotating;
    Vector3 current = Vector3.forward;

    private void Awake()
    {
        Assert.IsNotNull(objectToFollowPos);
        Assert.IsNotNull(objectToFollowRotate);
    }

    private void Start()
    {
        current = transform.forward;
    }

    private void LateUpdate()
    {
        Vector3 position = objectToFollowPos.position + transform.rotation * offset;
        this.transform.position = position;

        float angle = Vector3.Angle(current, objectToFollowRotate.forward);

        if (!rotating)
        {
            if (angle > rotateStartAngle)
                rotating = true;
        }
        else
        {
            if (angle <= rotateEndAngle)
                rotating = false;
        }

        if (!rotating)
            return;

        float currentSpeed = angle * rotationSpeed;
        float rotateAmount = currentSpeed * Time.deltaTime;

        Quaternion newRot = Quaternion.RotateTowards(transform.rotation, objectToFollowRotate.rotation, rotateAmount);
        transform.rotation = newRot;
        current = transform.forward;
    }
}
