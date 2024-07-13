using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PlaneCarrier : MonoBehaviour
{
    public float width = 1;
    public float length = 1;

    // Plane resolution
    public int widthResolution = 10;
    public int lengthResolution = 10;

    private void Start() {
        var mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        var vertices = new Vector3[(widthResolution + 1) * (lengthResolution + 1)];
        var uvs = new Vector2[(widthResolution + 1) * (lengthResolution + 1)];
        var triangles = new int[widthResolution * lengthResolution * 6];
        var unitWidth = width / widthResolution;
        var unitLength = length / lengthResolution;

        for (int i = 0, z = 0; z <= lengthResolution; z++) {
            for (int x = 0; x <= widthResolution; x++, i++) {
                vertices[i] = new Vector3(x * unitWidth, 0, z * unitLength);
                uvs[i] = new Vector2((float)x / widthResolution, (float)z / lengthResolution);
            }
        }

        var vert = 0;
        var tris = 0;
        for (int z = 0; z < lengthResolution; z++, vert++) {
            for (int x = 0; x < widthResolution; x++, vert++, tris += 6) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = triangles[tris + 4] = vert + widthResolution + 1;
                triangles[tris + 2] = triangles[tris + 3] = vert + 1;
                triangles[tris + 5] = vert + widthResolution + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        transform.position = new Vector3(-3, -5, 0);
    }
}
