
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{
    [Header("Warna Cone")]
    public Color normalColor = new Color(1f, 1f, 0f, 0.25f);
    public Color detectedColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("Pengaturan Cone")]
    public float viewDistance = 5f;
    public float viewAngle = 90f;
    public int sortingOrder = 1;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Vector2 currentDirection = Vector2.down;
    private const int Segments = 40;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "Vision Cone Mesh";
        meshFilter.sharedMesh = mesh;

        // Tampilkan mesh di depan lantai, tetapi di belakang sprite guard.
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = sortingOrder;

        // Aktifkan render dua sisi agar mesh tidak hilang akibat arah normal.
        meshRenderer.material.SetInt(
            "_Cull",
            (int)CullMode.Off
        );

        RebuildCone();
        SetNormal();
    }

    public void SetDirection(
        Vector2 direction,
        float distance,
        float angle)
    {
        if (direction.sqrMagnitude > 0.0001f)
            currentDirection = direction.normalized;

        viewDistance = distance;
        viewAngle = angle;

        RebuildCone();
    }

    void RebuildCone()
    {
        if (mesh == null)
            return;

        Vector3[] vertices = new Vector3[Segments + 2];
        int[] triangles = new int[Segments * 3];

        vertices[0] = Vector3.zero;

        float centerAngle = Mathf.Atan2(
            currentDirection.y,
            currentDirection.x
        ) * Mathf.Rad2Deg;

        for (int i = 0; i <= Segments; i++)
        {
            float angle = centerAngle
                - viewAngle / 2f
                + viewAngle * i / Segments;

            float radians = angle * Mathf.Deg2Rad;

            vertices[i + 1] = new Vector3(
                Mathf.Cos(radians) * viewDistance,
                Mathf.Sin(radians) * viewDistance,
                0f
            );
        }

        for (int i = 0; i < Segments; i++)
        {
            int index = i * 3;

            triangles[index] = 0;
            // Urutan dibalik untuk menghadap kamera 2D.
            triangles[index + 1] = i + 2;
            triangles[index + 2] = i + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public void SetNormal()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = normalColor;
    }

    public void SetDetected()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = detectedColor;
    }

    void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);
    }
}
