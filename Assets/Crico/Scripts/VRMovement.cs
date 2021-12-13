using UnityEngine;
using UnityEngine.Assertions;

namespace Crico.VR
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class VRMovement : MonoBehaviour
    {
        enum State
        {
            INVALID = -1,
            MANUAL_MOVEMENT,
            TURNING,
            MOVING_MARKER,
            WARPING,
        }

        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float jumpAccel = 6f;
        [SerializeField] float preJumpFloorCheckRayDist = 0.1f;
        [SerializeField] float warpDestSlopeMaxAngle = 30f;
        [SerializeField] float warpStickYThreshold = 0.4f;
        [SerializeField] float warpStickSpeedThreshold = 20f;
        [SerializeField] float warpTime = 0.3f;
        [SerializeField] float moveMarkerYOffset = 0.04f;
        [SerializeField] float moveMarkerMaxYMove = 1.1f;
        [SerializeField] float turnTime = 0.1f;
        [SerializeField] float turnAmount = 45f;
        [SerializeField] float turnStickThreshold = 0.5f;
        [SerializeField] string leftHorizontalAxisName = "LHorizontal";
        [SerializeField] string leftVerticalAxisName = "LVertical";
        [SerializeField] string rightHorizontalAxisName = "RHorizontal";
        [SerializeField] string rightVerticalAxisName = "RVertical";
        [SerializeField] string jumpButtonName = "Confirm";
        [SerializeField] GameObject moveMarker = null;
        [SerializeField] ProjectilePath movePath = null;
        [SerializeField] Material movePathInvalidMat = null;
        [SerializeField] Transform cameraTransform = null;
        [SerializeField] Transform[] parts = new Transform[] { };

        State state = State.INVALID;

        // MANUAL MOVEMENT
        bool triggerJump;

        // TURNING
        float currentTurnTime;
        Quaternion turnOrigin;
        Quaternion turnTarget;

        // MOVING MARKER
        Vector2 lastRStick;
        Vector3 moveMarkerPos;
        Material movePathMat;
        bool canMove;

        // WARPING
        Vector3 warpDest;
        Vector3 warpOrigin;
        float currentWarpTime;

        private void Awake()
        {
            Assert.IsNotNull(moveMarker);
            Assert.IsNotNull(movePath);
            Assert.IsNotNull(movePathInvalidMat);
            Assert.IsNotNull(cameraTransform);
            Assert.IsNotNull(GetComponent<CapsuleCollider>());
            Assert.IsNotNull(GetComponent<Rigidbody>());

            movePathMat = movePath.GetComponent<MeshRenderer>().material;
        }

        private void Start()
        {
            moveMarker.SetActive(false);
            movePath.gameObject.SetActive(false);
            EnterManualMovement();
        }

        private void FixedUpdate()
        {
            switch (state)
            {
                case State.MANUAL_MOVEMENT:
                    FixedUpdateManualMovement();
                    break;

                case State.MOVING_MARKER:
                    FixedUpdateMovingMarker();
                    break;

                case State.TURNING:
                case State.WARPING:
                default:
                    break;
            }
        }


        private void Update()
        {
            switch (state)
            {
                case State.MANUAL_MOVEMENT:
                    UpdateManualMovement();
                    break;

                case State.MOVING_MARKER:
                    UpdateMovingMarker();
                    break;

                case State.TURNING:
                    UpdateTurning();
                    break;

                case State.WARPING:
                    UpdateWarping();
                    break;

                default:
                    break;
            }

            if (!triggerJump)
                triggerJump = Input.GetButtonDown(jumpButtonName);
        }

        Vector2 GetLeftStick()
        {
            Vector2 lStick = new Vector2(Input.GetAxis(leftHorizontalAxisName), -Input.GetAxis(leftVerticalAxisName));
            return lStick;
        }

        Vector2 GetRightStick()
        {
            Vector2 rStick = new Vector2(Input.GetAxis(rightHorizontalAxisName), -Input.GetAxis(rightVerticalAxisName));
            return rStick;
        }

        // *******************
        // MANUAL MOVEMENT
        // *******************

        void EnterManualMovement()
        {
            Exit();
            state = State.MANUAL_MOVEMENT;
            triggerJump = false;
        }

        void UpdateManualMovement()
        {
            Vector2 rStick = GetRightStick();

            float rhAbs = Mathf.Abs(rStick.x);
            float rhPol = rhAbs / rStick.x;

            if (rhAbs >= turnStickThreshold)
            {
                EnterTurning(rhPol);
                return;
            }
            else if (rStick.y >= turnStickThreshold)
            {
                EnterMovingMarker();
                return;
            }
        }

        void FixedUpdateManualMovement()
        {
            Vector2 lStick = GetLeftStick();

            Vector3 moveDir = (lStick.x * cameraTransform.right + lStick.y * cameraTransform.forward);
            moveDir.y = 0f;
            moveDir.Normalize();

            Vector3 targetVelocity = moveDir * moveSpeed;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            Vector3 velocity = rigidbody.velocity;
            Vector3 deltaVelocity = targetVelocity - velocity;
            deltaVelocity.y = 0f;

            if (deltaVelocity.sqrMagnitude > 0.001f)
                rigidbody.AddForce(deltaVelocity, ForceMode.VelocityChange);

            if (triggerJump)
            {
                triggerJump = false;

                Vector3 floorCheckOrigin = transform.position;
                floorCheckOrigin.y += preJumpFloorCheckRayDist;

                bool floorHit = Physics.Raycast(floorCheckOrigin, Vector3.down, preJumpFloorCheckRayDist * 2f);

                if (floorHit)
                    rigidbody.AddForce(0f, jumpAccel, 0f, ForceMode.VelocityChange);
            }
        }

        // *******************
        // TURNING
        // *******************

        void EnterTurning(float dir)
        {
            Exit();

            state = State.TURNING;

            currentTurnTime = 0f;

            float newTurnAmount = turnAmount * dir;
            turnOrigin = transform.rotation;
            turnTarget = Quaternion.Euler(0f, newTurnAmount, 0f) * transform.rotation;
        }

        void UpdateTurning()
        {
            currentTurnTime += Time.deltaTime;

            if (currentTurnTime >= turnTime)
            {
                currentTurnTime = turnTime;
                transform.rotation = turnTarget;

                Vector2 rStick = GetRightStick();
                float rhAbs = Mathf.Abs(rStick.x);

                if (rhAbs < turnStickThreshold)
                    EnterManualMovement();

                return;
            }

            float t = currentTurnTime / turnTime;

            Quaternion current = Quaternion.Lerp(turnOrigin, turnTarget, t);

            transform.rotation = current;
        }

        // *******************
        // MOVING MARKER
        // *******************

        void EnterMovingMarker()
        {
            Exit();

            state = State.MOVING_MARKER;

            moveMarker.SetActive(true);
            movePath.gameObject.SetActive(true);
            canMove = false;

            moveMarkerPos = transform.position;
            RefreshMarkerVisualPos();

            lastRStick = GetRightStick();
        }

        void RefreshMarkerVisualPos()
        {
            moveMarker.transform.position = moveMarkerPos + Vector3.up * moveMarkerYOffset;
        }

        void FixedUpdateMovingMarker()
        {
            Vector3 pathEnd = movePath.GetFinalPosition();
            Vector3 normal = movePath.finalPositionNormal;

            float yDist = Mathf.Abs(pathEnd.y - transform.position.y);


            CapsuleCollider collider = GetComponent<CapsuleCollider>();

            Vector3 capsuleBottom = pathEnd;
            capsuleBottom.y += moveMarkerYOffset + collider.radius;

            Vector3 capsuleTop = capsuleBottom;
            capsuleTop.y += collider.height - 2f * collider.radius;

            bool capsuleHit = Physics.OverlapCapsule(capsuleBottom, capsuleTop, collider.radius).Length > 0;

            canMove = Vector3.Angle(normal, Vector3.up) <= warpDestSlopeMaxAngle
                        && yDist <= moveMarkerMaxYMove
                        && !capsuleHit;

            moveMarker.SetActive(canMove);
            moveMarkerPos = pathEnd;
            moveMarkerPos.y += moveMarkerYOffset;

            moveMarker.transform.position = moveMarkerPos;

            Material matToUse = canMove ? movePathMat : movePathInvalidMat;

            MeshRenderer pathRenderer = movePath.GetComponent<MeshRenderer>();
            if (pathRenderer.material != matToUse)
                pathRenderer.material = matToUse;
        }

        void UpdateMovingMarker()
        {
            Vector2 rStick = GetRightStick();

            Vector2 stickMove = rStick - lastRStick;

            float stickSpeed = Mathf.Abs(stickMove.y / Time.deltaTime);

            lastRStick = rStick;

            if (rStick.y < warpStickYThreshold)
            {
                if (stickSpeed >= warpStickSpeedThreshold && canMove)
                {
                    EnterWarping(moveMarkerPos);
                }
                else
                {
                    EnterManualMovement();
                }
            }
        }

        void ExitMovingMarker()
        {
            moveMarker.SetActive(false);
            movePath.gameObject.SetActive(false);
        }

        // *******************
        // WARPING
        // *******************

        void EnterWarping(Vector3 warpDest)
        {
            Exit();

            state = State.WARPING;

            currentWarpTime = 0f;

            warpOrigin = transform.position;
            this.warpDest = warpDest;

            Vector3 dist = warpDest - warpOrigin;
            float speed = dist.magnitude / warpTime;
            Vector3 velocity = dist.normalized * speed;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = velocity;
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
        }

        void UpdateWarping()
        {
            currentWarpTime += Time.deltaTime;

            if (currentWarpTime >= warpTime)
            {
                EnterManualMovement();
                return;
            }
        }

        void ExitWarping()
        {
            transform.position = warpDest;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.ResetInertiaTensor();
            rigidbody.detectCollisions = true;
            rigidbody.useGravity = true;
        }

        // *******************
        // EXIT
        // *******************

        void Exit()
        {
            switch (state)
            {
                case State.MANUAL_MOVEMENT:
                case State.TURNING:
                default:
                    break;

                case State.WARPING:
                    ExitWarping();
                    break;
                case State.MOVING_MARKER:
                    ExitMovingMarker();
                    break;
            }
        }

    }

}
