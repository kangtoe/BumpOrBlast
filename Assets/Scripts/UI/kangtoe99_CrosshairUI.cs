using UnityEngine;

public class kangtoe99_CrosshairUI : MonoBehaviour
{
    public static kangtoe99_CrosshairUI Instance { get; private set; }

    [Header("Crosshair Settings")]
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float lineLength = 10f;
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private float centerGap = 4f;
    [SerializeField] private bool showCenterDot = true;
    [SerializeField] private float dotSize = 2f;

    [Header("Outline")]
    [SerializeField] private bool showOutline = true;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineThickness = 1f;

    [Header("Cursor")]
    [SerializeField] private bool hideCursor = true;

    [Header("Spread Animation")]
    [SerializeField] private float spreadGap = 15f;
    [SerializeField] private float spreadSpeed = 10f;
    [SerializeField] private float recoverSpeed = 8f;

    private Texture2D crosshairTexture;
    private bool isVisible = true;
    private float currentGap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CreateTexture();
        currentGap = centerGap;

        if (hideCursor)
        {
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // 마우스 버튼 누르면 갭 벌어짐, 떼면 원래대로 (타임스케일 영향 없음)
        float targetGap = Input.GetMouseButton(0) ? spreadGap : centerGap;
        float speed = Input.GetMouseButton(0) ? spreadSpeed : recoverSpeed;
        currentGap = Mathf.Lerp(currentGap, targetGap, speed * Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void CreateTexture()
    {
        crosshairTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        crosshairTexture.SetPixel(0, 0, Color.white);
        crosshairTexture.Apply();
    }

    private void OnDestroy()
    {
        if (crosshairTexture != null)
        {
            Destroy(crosshairTexture);
        }
    }

    private void OnGUI()
    {
        if (!isVisible || crosshairTexture == null) return;

        // 마우스 커서 위치 (Y축 반전 필요 - Input은 좌하단 원점, GUI는 좌상단 원점)
        Vector2 mousePos = Input.mousePosition;
        Vector2 center = new Vector2(mousePos.x, Screen.height - mousePos.y);

        // Draw outline first (if enabled)
        if (showOutline)
        {
            DrawCrosshair(center, outlineColor, outlineThickness);
        }

        // Draw main crosshair
        DrawCrosshair(center, crosshairColor, 0f);
    }

    private void DrawCrosshair(Vector2 center, Color color, float outlineOffset)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 빌드에서만 linear 변환 적용 (이중 gamma 보정 상쇄)
        GUI.color = color.linear;
#else
        GUI.color = color;
#endif

        float thickness = lineThickness + outlineOffset * 2;
        float length = lineLength + outlineOffset * 2;
        float gap = currentGap - outlineOffset;
        float dot = dotSize + outlineOffset * 2;

        // Horizontal line - Left
        GUI.DrawTexture(new Rect(
            center.x - gap - length,
            center.y - thickness / 2,
            length,
            thickness
        ), crosshairTexture);

        // Horizontal line - Right
        GUI.DrawTexture(new Rect(
            center.x + gap,
            center.y - thickness / 2,
            length,
            thickness
        ), crosshairTexture);

        // Vertical line - Top
        GUI.DrawTexture(new Rect(
            center.x - thickness / 2,
            center.y - gap - length,
            thickness,
            length
        ), crosshairTexture);

        // Vertical line - Bottom
        GUI.DrawTexture(new Rect(
            center.x - thickness / 2,
            center.y + gap,
            thickness,
            length
        ), crosshairTexture);

        // Center dot
        if (showCenterDot)
        {
            GUI.DrawTexture(new Rect(
                center.x - dot / 2,
                center.y - dot / 2,
                dot,
                dot
            ), crosshairTexture);
        }

        GUI.color = Color.white;
    }

    #region Public Methods

    public void SetVisible(bool visible)
    {
        isVisible = visible;
    }

    public void SetColor(Color color)
    {
        crosshairColor = color;
    }

    public void SetSize(float length, float thickness)
    {
        lineLength = length;
        lineThickness = thickness;
    }

    #endregion
}
