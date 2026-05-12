using UnityEngine;

[DefaultExecutionOrder(100)]
public class kangtoe99_GridBackground : MonoBehaviour
{
    [Header("Grid Visual")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int cellPixelResolution = 32;
    [SerializeField] private int linePixelThickness = 3;
    [SerializeField] private Color lineColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private Color bgColor = new Color(0.07f, 0.07f, 0.07f, 1f);

    [Header("Coverage")]
    [SerializeField] private Vector2 coverageSize = new Vector2(100f, 100f);
    [SerializeField] private int sortingOrder = -100;
    [SerializeField] private string sortingLayerName = "Default";

    [Header("Follow")]
    [SerializeField] private Transform followTarget;

    private Material gridMaterial;
    private Texture2D gridTexture;

    private void Awake()
    {
        Vector2 tiling = new Vector2(coverageSize.x / cellSize, coverageSize.y / cellSize);

        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = CreateQuadMesh(coverageSize, tiling);

        var mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

        gridTexture = GenerateGridTexture();
        gridMaterial = CreateGridMaterial(gridTexture);
        mr.sharedMaterial = gridMaterial;

        mr.sortingOrder = sortingOrder;
        mr.sortingLayerName = sortingLayerName;

        if (followTarget == null && Camera.main != null)
        {
            followTarget = Camera.main.transform;
        }

        Debug.Log($"[GridBackground] 초기화 완료 — coverage={coverageSize}, cell={cellSize}, UV tiling={tiling}, shader={gridMaterial.shader.name}");
    }

    private void OnDestroy()
    {
        if (gridMaterial != null) Destroy(gridMaterial);
        if (gridTexture != null) Destroy(gridTexture);
    }

    private Material CreateGridMaterial(Texture2D tex)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null)
        {
            Debug.LogError("[GridBackground] 호환 가능한 shader를 찾지 못했습니다.");
            return null;
        }

        var mat = new Material(shader) { name = "GridMaterial" };
        Vector2 tiling = new Vector2(coverageSize.x / cellSize, coverageSize.y / cellSize);

        // URP/Legacy 양쪽 프로퍼티 이름 모두 시도
        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex", tex);
            mat.SetTextureScale("_MainTex", tiling);
        }
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", tiling);
        }
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

        return mat;
    }

    // UV를 0~tiling 범위로 직접 확장 → shader의 _MainTex_ST 무시 여부와 무관하게 타일링 적용
    private Mesh CreateQuadMesh(Vector2 size, Vector2 tiling)
    {
        var mesh = new Mesh { name = "GridQuad" };
        float hx = size.x * 0.5f;
        float hy = size.y * 0.5f;
        mesh.vertices = new Vector3[]
        {
            new Vector3(-hx, -hy, 0f),
            new Vector3(hx, -hy, 0f),
            new Vector3(hx, hy, 0f),
            new Vector3(-hx, hy, 0f)
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(tiling.x, 0f),
            new Vector2(tiling.x, tiling.y),
            new Vector2(0f, tiling.y)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        return mesh;
    }

    private Texture2D GenerateGridTexture()
    {
        int size = cellPixelResolution;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Point,
            name = "GridTexture"
        };

        var pixels = new Color[size * size];
        int t = Mathf.Clamp(linePixelThickness, 1, size / 2);
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            bool onLine = x < t || y < t;
            pixels[i] = onLine ? lineColor : bgColor;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;
        Vector3 p = followTarget.position;
        float sx = Mathf.Floor(p.x / cellSize) * cellSize;
        float sy = Mathf.Floor(p.y / cellSize) * cellSize;
        transform.position = new Vector3(sx, sy, transform.position.z);
    }

    public void SetFollowTarget(Transform t) => followTarget = t;
}
