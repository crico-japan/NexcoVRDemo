using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class Hand : MonoBehaviour
{
    [SerializeField] Rigidbody parentBody = null;
    [SerializeField] Transform followTargetController = null;
    [SerializeField] float handFollowSpeed = 30f;
    [SerializeField] Vector3 handRotationOffset;
    [SerializeField] float handRotateSpeed = 100f;
    [SerializeField] float maxAngularVelocity = 20f;
    [SerializeField] float gripAnimSpeed = 10f;
    [SerializeField] float minGrabGrip = 0.3f;
    [SerializeField] float maxGrabGrip = 0.8f;
    [SerializeField] float gripCheckSphereRadius = 0.1f; 
    [SerializeField] Transform palmPosition = null;
    [SerializeField] LayerMask uiLayer = new LayerMask();
    [SerializeField] float distToCheckForUI = 10f;
    [SerializeField] string triggerFieldName = "trigger";
    [SerializeField] string gripFieldName = "grip";

    new Rigidbody rigidbody;

    bool grippingObject;
    GameObject grippedObject;
    int grippedObjectLayer;
    FixedJoint objectJoint;
    FixedJoint thisJoint;

    Animator animator;
    float triggerTarget;
    float gripTarget;
    float gripCurrent;
    float triggerCurrent;

    Transform followTarget;


    private void Awake()
    {
        Assert.IsNotNull(palmPosition);
        Assert.IsNotNull(followTargetController);
        this.rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        followTarget = followTargetController;
        rigidbody.maxAngularVelocity = maxAngularVelocity;
    }


    private void FixedUpdate()
    {
        PhysicsHand();
        GripHand();
        UICheck();
    }

    private void UICheck()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, distToCheckForUI, uiLayer.value);

        if (hits.Length == 0)
            return;

        int validHitIndex = -1;

        float closestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit hit = hits[i];

            if (Vector3.Dot(-hit.collider.transform.forward, (transform.position - hit.point)) <= 0)
                continue;

            float dist = (transform.position - hit.point).magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                validHitIndex = i;
            }
        }

        if (validHitIndex < 0)
            return;

        RaycastHit validHit = hits[validHitIndex];
        Canvas canvas = validHit.collider.GetComponent<Canvas>();
        if (canvas == null)
            return;

        Vector3 relativePoint = validHit.point - canvas.transform.position;

        relativePoint = Quaternion.Inverse(canvas.transform.rotation) * relativePoint;

        relativePoint.x /= canvas.transform.localScale.x;
        relativePoint.y /= canvas.transform.localScale.y;

        Debug.Log("Hit point: " + relativePoint);
    }

    void Update()
    {
        AnimateHand();
    }

    internal void SetTrigger(float trigger)
    {
        triggerTarget = trigger;
    }

    bool IsGrabGrip(float value)
    {
        return value >= minGrabGrip && value <= maxGrabGrip;
    }

    internal void SetGrip(float grip)
    {
        gripTarget = grip;

        // there is a grip window of between x and y where you can grip an object
        // once grabbed, animation ceases
        // joints are added to the object, its collision mask is stored then changed
        // object is moved to palm location
    }

    void GripHand()
    {
        if (grippingObject)
        {
            if (gripTarget < minGrabGrip)
            {
                // release object
                grippedObject.layer = grippedObjectLayer;
                grippedObjectLayer = -1;

                grippedObject = null;

                Destroy(thisJoint);
                thisJoint = null;

                Destroy(objectJoint);
                objectJoint = null;

                grippingObject = false;
            }
        }
        else
        {
            if (IsGrabGrip(gripCurrent))
            {
                // check for grab object
                Collider[] overlaps = Physics.OverlapSphere(palmPosition.position, gripCheckSphereRadius);
                if (overlaps.Length > 0)
                {
                    Collider grabObjectCollider = null;
                    Rigidbody grabObjectBody = null;

                    foreach (Collider overlap in overlaps)
                    {
                        if (overlap.gameObject.layer == gameObject.layer)
                            continue;

                        Rigidbody body = overlap.GetComponent<Rigidbody>();
                        if (body != null)
                        {
                            grabObjectBody = body;
                            grabObjectCollider = overlap;
                            break;
                        }
                    }

                    if (grabObjectCollider != null)
                    {
                        grippedObject = grabObjectCollider.gameObject;

                        Vector3 closetPoint = grabObjectCollider.ClosestPoint(palmPosition.position);
                        Vector3 dist = palmPosition.position - closetPoint;
                        grippedObject.transform.position = grippedObject.transform.position + dist;

                        thisJoint = gameObject.AddComponent<FixedJoint>();
                        thisJoint.connectedBody = grabObjectBody;
                        thisJoint.enableCollision = false;
                        //thisJoint.connectedMassScale = 0f;
                        thisJoint.breakForce = float.PositiveInfinity;
                        thisJoint.breakTorque = float.PositiveInfinity;

                        objectJoint = grippedObject.AddComponent<FixedJoint>();
                        objectJoint.connectedBody = GetComponent<Rigidbody>();
                        objectJoint.enableCollision = false;
                        //objectJoint.connectedMassScale = 0f;
                        objectJoint.breakForce = float.PositiveInfinity;
                        objectJoint.breakTorque = float.PositiveInfinity;

                        grippedObjectLayer = grippedObject.layer;
                        grippedObject.layer = gameObject.layer;

                        grippingObject = true;
                    }
                }
            }
        }
    }

    void AnimateHand()
    {
        if (grippingObject)
            return;

        if (gripCurrent != gripTarget)
        {
            gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * gripAnimSpeed);
            animator.SetFloat(gripFieldName, gripCurrent);
        }

        if (triggerCurrent != triggerTarget)
        {
            triggerCurrent = Mathf.MoveTowards(triggerCurrent, triggerTarget, Time.deltaTime * gripAnimSpeed);
            animator.SetFloat(triggerFieldName, triggerCurrent);
        }
    }

    void PhysicsHand()
    {
        Vector3 targetPos = followTarget.position;
        Vector3 distVec = targetPos - transform.position;
        Vector3 targetVel = distVec * handFollowSpeed;
        
        if (parentBody != null)
            targetVel += parentBody.velocity;

        rigidbody.velocity = targetVel;

        Quaternion targetRotation = followTarget.rotation * Quaternion.Euler(handRotationOffset);
        Quaternion rotDiff = targetRotation * Quaternion.Inverse(rigidbody.rotation);
        float angle = 0f;
        Vector3 axis = Vector3.zero;
        rotDiff.ToAngleAxis(out angle, out axis);
        rigidbody.angularVelocity = axis * (angle * Mathf.Deg2Rad * handRotateSpeed);
    }
}
