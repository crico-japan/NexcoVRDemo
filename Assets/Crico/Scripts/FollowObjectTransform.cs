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
    Quaternion currentRot = Quaternion.identity;

    private void Awake()
    {
        Assert.IsNotNull(objectToFollowPos);
        Assert.IsNotNull(objectToFollowRotate);
    }

    private void Start()
    {
        currentRot = Quaternion.LookRotation(transform.forward);
    }

    private void LateUpdate()
    {
        Vector3 position = objectToFollowPos.position + currentRot * offset;
        this.transform.position = position;

        Vector3 currentForward = transform.forward;
        float angle = Vector3.Angle(currentForward, objectToFollowRotate.forward);

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
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, transform.up);
        Quaternion newRot = Quaternion.RotateTowards(targetRotation, objectToFollowRotate.rotation, rotateAmount);
        transform.rotation = newRot;
        currentRot = newRot;
    }
}
