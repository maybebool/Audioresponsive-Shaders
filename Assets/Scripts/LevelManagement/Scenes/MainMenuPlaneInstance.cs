using UnityEngine;

namespace LevelManagement.Scenes {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MainMenuPlaneInstance : MonoBehaviour {
        [SerializeField] private float width = 1;
        [SerializeField] private float length = 1;

        [SerializeField] private int widthResolution = 10;
        [SerializeField] private int lengthResolution = 10;
        
        private void Start() {
            var mesh = GenerateNewMesh(widthResolution, lengthResolution);
            GetComponent<MeshFilter>().mesh = mesh;
            transform.position = new Vector3(-3, -5, 0);
        }

        /// <summary>
        /// Generates a new Mesh for the MainMenuPlaneInstance.
        /// </summary>
        /// <param name="widthRes">The number of divisions along the width of the plane.</param>
        /// <param name="lengthRes">The number of divisions along the length of the plane.</param>
        /// <returns>A new Mesh object with the generated vertices, UVs, and triangles.</returns>
        private Mesh GenerateNewMesh(int widthRes, int lengthRes) {
            var generatedMesh = new Mesh();
            var vertices = GenerateVertices(widthRes, lengthRes);
            var uvs = GenerateUvs(widthRes, lengthRes);
            var triangles = GenerateTriangles(widthRes, lengthRes);

            generatedMesh.vertices = vertices;
            generatedMesh.uv = uvs;
            generatedMesh.triangles = triangles;
            generatedMesh.RecalculateNormals();

            return generatedMesh;
        }

        /// <summary>
        /// Generates the vertices for the MainMenuPlaneInstance mesh.
        /// </summary>
        /// <param name="widthRes">The number of divisions along the width of the plane.</param>
        /// <param name="lengthRes">The number of divisions along the length of the plane.</param>
        /// <returns>A Vector3 array containing the generated vertices.</returns>
        private Vector3[] GenerateVertices(int widthRes, int lengthRes) {
            var unitWidth = width / widthRes;
            var unitLength = length / lengthRes;
            var vertices = new Vector3[(widthRes + 1) * (lengthRes + 1)];

            for (int i = 0, z = 0; z <= lengthRes; z++) {
                for (int x = 0; x <= widthRes; x++, i++) {
                    vertices[i] = new Vector3(x * unitWidth, 0, z * unitLength);
                }
            }

            return vertices;
        }

        /// <summary>
        /// Generates the UV coordinates for the MainMenuPlaneInstance mesh.
        /// </summary>
        /// <param name="widthRes">The number of divisions along the width of the plane.</param>
        /// <param name="lengthRes">The number of divisions along the length of the plane.</param>
        /// <returns>A Vector2 array containing the generated UV coordinates.</returns>
        private Vector2[] GenerateUvs(int widthRes, int lengthRes) {
            var uvs = new Vector2[(widthRes + 1) * (lengthRes + 1)];

            for (int i = 0, z = 0; z <= lengthRes; z++) {
                for (int x = 0; x <= widthRes; x++, i++) {
                    uvs[i] = new Vector2((float)x / widthRes, (float)z / lengthRes);
                }
            }

            return uvs;
        }

        /// <summary>
        /// Generates the triangles for the MainMenuPlaneInstance mesh.
        /// </summary>
        /// <param name="widthRes">The number of divisions along the width of the plane.</param>
        /// <param name="lengthRes">The number of divisions along the length of the plane.</param>
        /// <returns>An int array containing the generated triangles.</returns>
        private int[] GenerateTriangles(int widthRes, int lengthRes) {
            var triangles = new int[widthRes * lengthRes * 6];
            var vert = 0;
            var tris = 0;

            for (int z = 0; z < lengthRes; z++, vert++) {
                for (int x = 0; x < widthRes; x++, vert++, tris += 6) {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = triangles[tris + 4] = vert + widthRes + 1;
                    triangles[tris + 2] = triangles[tris + 3] = vert + 1;
                    triangles[tris + 5] = vert + widthRes + 2;
                }
            }

            return triangles;
        }
    }
}