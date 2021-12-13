using UnityEngine;

namespace Crico
{
    [RequireComponent(typeof(StripMesh))]
    public class BezierStrip : MonoBehaviour
    {
        [SerializeField] int numSegments = 16;
        [SerializeField] Vector3 p0Offset = new Vector3();
        [SerializeField] Vector3 p1Offset = new Vector3(0f, 1f, 1f);
        [SerializeField] Vector3 p2Offset = new Vector3(0f, 1f, -1f);
        [SerializeField] Vector3 p3Offset = new Vector3();
        [SerializeField] Vector3 p3Base = new Vector3(0f, 0f, 6f);

        Vector3 currentP3Base;
        Vector3[] centres;

        private void Awake()
        {
            centres = new Vector3[numSegments + 1];
        }

        public void SetEndPoint(Vector3 endPoint)
        {
            p3Base = endPoint;
        }

        void CalcPoints()
        {
            if (currentP3Base == p3Base)
                return;

            currentP3Base = p3Base;

            Vector3 p0 = p0Offset.x * transform.right + p0Offset.y * transform.up + p0Offset.z * transform.forward;
            Vector3 p3 = p3Base + p3Offset.x * transform.right + p3Offset.y * transform.up + p3Offset.z * transform.forward;

            Vector3 dir = p3 - p0;
            dir.y = 0f;
            dir.Normalize();

            Vector3 right = Vector3.Cross(transform.up, dir).normalized;

            Vector3 p1 = p0 + right * p1Offset.x + transform.up * p1Offset.y + dir * p1Offset.z;
            Vector3 p2 = p3 + right * p2Offset.x + transform.up * p2Offset.y + dir * p2Offset.z;

            float step = 1f / (float)numSegments;

            for (int i = 0; i < numSegments + 1; ++i)
            {
                float t = (float)i / (float)numSegments;
                centres[i] = CalcPointAt(t, p0, p1, p2, p3);
            }

            GetComponent<StripMesh>().InitMesh(centres, Vector3.zero);
        }

        static Vector3 CalcPointAt(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float oneMinusT = 1f - t;

            Vector3 result = Mathf.Pow(oneMinusT, 3f) * p0
                + 3f * Mathf.Pow(oneMinusT, 2f) * t * p1
                + 3f * oneMinusT * t * t * p2
                + t * t * t * p3;

            return result;
        }

        private void Update()
        {
            CalcPoints();
        }

    }

}
