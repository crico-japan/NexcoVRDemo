using UnityEngine;

namespace Crico
{
    [RequireComponent(typeof(StripMesh))]
    public class ProjectilePath : MonoBehaviour
    {
        [SerializeField] float initialSpeed = 10f;
        [SerializeField] int maxPointCalcs = 16;
        [SerializeField] float gravity = 9.81f;
        [SerializeField] float minY = -1f;

        Vector3[] positions;
        int numPositions;

        Vector3 lastPosition;
        Quaternion lastRotation;

        bool triggerMeshRefresh;

        public Vector3 finalPositionNormal { get; private set; }

        private void Awake()
        {
            positions = new Vector3[maxPointCalcs];
        }

        public Vector3 GetFinalPosition()
        {
            if (numPositions > 0)
                return positions[numPositions - 1];

            return transform.position;
        }

        public void CalcPathPoints()
        {
            Vector3 result = transform.position;

            Vector3 velocity = transform.forward * initialSpeed;

            float upTime = velocity.y / gravity;

            float yMax = transform.position.y + velocity.y * upTime + 0.5f * gravity * upTime * upTime;

            float yDownDist = yMax - minY;
            float downTime = Mathf.Sqrt(2f * yDownDist / gravity);

            float maxTime = upTime + downTime;

            int numSegments = maxPointCalcs - 1;
            float timeStep = maxTime / (float)numSegments;

            positions[0] = transform.position;
            Vector3 horz = velocity;
            horz.y = 0f;
            float theta = Vector3.Angle(velocity, horz) * Mathf.Deg2Rad;

            numPositions = positions.Length;

            for (int i = 1; i < maxPointCalcs; ++i)
            {
                float t = (float)i / (float)numSegments * maxTime;
                Vector3 position = transform.position + horz * Mathf.Cos(theta) * t
                    + Vector3.up * ((velocity.y * Mathf.Sin(theta) * t) - 0.5f * gravity * t * t);


                Vector3 lineStart = positions[i - 1];

                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Linecast(lineStart, position, out hitInfo);
                
                if (hit)
                {
                    position = hitInfo.point;
                    positions[i] = position;
                    finalPositionNormal = hitInfo.normal;

                    for (int j = i + 1; j < maxPointCalcs; ++j)
                    {
                        positions[j] = position;
                    }
                    break;
                }
                else
                {
                    positions[i] = position;
                    finalPositionNormal = Vector3.up;
                }
            }
        }

        private void FixedUpdate()
        {
            if (transform.position != lastPosition
                || transform.rotation != lastRotation)
            {
                lastPosition = transform.position;
                lastRotation = transform.rotation;

                CalcPathPoints();
                triggerMeshRefresh = true;
            }
        }

        private void Update()
        {
            if (triggerMeshRefresh)
            {
                triggerMeshRefresh = false;
                RefreshMesh();
            }
        }

        private void RefreshMesh()
        {
            GetComponent<StripMesh>().InitMesh(positions, -transform.position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < numPositions; ++i)
            {
                Vector3 position = positions[i];
                Gizmos.DrawWireSphere(position, 0.1f);
            }

            for (int i = 1; i < numPositions; ++i)
            {
                Vector3 start = positions[i - 1];
                Vector3 position = positions[i];
                Gizmos.DrawLine(start, position);
            }

        }
    }

}
