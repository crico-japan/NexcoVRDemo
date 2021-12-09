using UnityEngine;

namespace Crico
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class StripMesh : MonoBehaviour
    {
        [SerializeField] float stripWidth = 0.1f;
        [SerializeField] float textureScrollSpeed = 0f;
        [SerializeField] string materialTextureFieldName = "_MainTex";

        int numFaces = -1;
        Vector3[] verts;
        Vector3[] segmentNormals;
        Vector3[] normals;
        Vector3[] centres;
        Vector2[] uvs;
        int[] indices;

        float textureOffset;

        public void InitMesh(Vector3[] centres, Vector3 offset)
        {
            int numCentres = centres.Length;
            int numSegments = numCentres - 1;
            int newNumFaces = numSegments * 2;

            if (newNumFaces != numFaces)
            {
                numFaces = newNumFaces;

                int numVerts = numSegments * 2 + 2;
                int numIndices = numFaces * 3;

                verts = new Vector3[numVerts];
                segmentNormals = new Vector3[numSegments];
                normals = new Vector3[numVerts];
                indices = new int[numIndices];
                centres = new Vector3[numSegments + 1];
                uvs = new Vector2[numVerts];
            }

            for (int i = 0; i < numSegments + 1; ++i)
            {
                int uvIndex = i * 2;
                float t = (float)i / (float)numSegments;

                uvs[uvIndex] = new Vector2(0f, t);
                uvs[uvIndex + 1] = new Vector2(1f, t);
            }

            float halfWidth = stripWidth / 2f;

            Vector3 dir = TransformPoint(centres[numCentres - 1], offset) - TransformPoint(centres[0], offset);
            dir.y = 0f;
            dir.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

            Quaternion inverse = Quaternion.Inverse(transform.rotation);

            int vertIndex = 0;
            for (int i = 0; i < numSegments + 1; ++i)
            {
                Vector3 centre = TransformPoint(centres[i], offset);

                Vector3 vLeft =  inverse * (centre - right * halfWidth);
                Vector3 vRight = inverse * (centre + right * halfWidth);

                verts[vertIndex++] = vLeft;
                verts[vertIndex++] = vRight;
            }

            int indiceIndex = 0;
            for (int i = 0; i < numSegments; ++i)
            {
                int i0 = i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;

                indices[indiceIndex++] = i0;
                indices[indiceIndex++] = i2;
                indices[indiceIndex++] = i1;

                indices[indiceIndex++] = i2;
                indices[indiceIndex++] = i3;
                indices[indiceIndex++] = i1;
            }

            for (int i = 0; i < numSegments; ++i)
            {
                int i0 = i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;

                Vector3 v0 = verts[i0];
                Vector3 v1 = verts[i1];
                Vector3 v2 = verts[i2];

                Vector3 normal = Vector3.Cross((v2 - v0), (v1 - v0)).normalized;

                segmentNormals[i] = normal;
            }

            int normalIndex = 0;
            for (int i = 0; i < numSegments + 1; ++i)
            {
                Vector3 normal;

                if (i == 0)
                {
                    // segment normal
                    normal = segmentNormals[i];
                }
                else if (i == numSegments)
                {
                    normal = segmentNormals[i - 1];
                }
                else
                {
                    // avg of face normals
                    int prevIndex = i - 1;
                    normal = segmentNormals[prevIndex] + segmentNormals[i];
                    normal.Normalize();
                }

                normals[normalIndex++] = normal;
                normals[normalIndex++] = normal;
            }

            MeshFilter meshFilter = GetComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.uv = uvs;

            meshFilter.mesh = mesh;
        }

        static Vector3 TransformPoint(Vector3 input, Vector3 offset)
        {
            return (input + offset);
        }

        private void Update()
        {
            ScrollTexture();
        }

        void ScrollTexture()
        {
            if (textureScrollSpeed == 0f)
                return;

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Material material = meshRenderer.material;

            textureOffset += textureScrollSpeed * Time.deltaTime;
            textureOffset = textureOffset % 1f;
            material.SetTextureOffset(materialTextureFieldName, new Vector2(0f, textureOffset));

            meshRenderer.material = material;
        }


    }

}
