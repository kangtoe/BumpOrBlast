using UnityEngine;

/// <summary>
/// 카메라의 종횡비를 고정하고 레터박스/필러박스를 추가합니다.
/// </summary>
public class kangtoe99_AspectRatioController : MonoBehaviour
{
    [Header("Aspect Ratio Settings")]
    [Tooltip("목표 종횡비 (너비:높이)")]
    public Vector2 targetAspect = new Vector2(16, 9);

    [Header("Letterbox Settings")]
    [Tooltip("레터박스 색상")]
    public Color letterboxColor = Color.black;

    private Camera cam;
    private float targetAspectRatio;

    /// <summary>
    /// 현재 활성화된 AspectRatioController의 목표 종횡비를 반환합니다.
    /// AspectRatioController가 없으면 카메라의 실제 aspect ratio를 반환합니다.
    /// </summary>
    public static float GetEffectiveAspectRatio(Camera camera)
    {
        if (camera == null) return Camera.main != null ? Camera.main.aspect : 16f / 9f;

        kangtoe99_AspectRatioController controller = camera.GetComponent<kangtoe99_AspectRatioController>();
        if (controller != null && controller.enabled)
        {
            return controller.targetAspectRatio;
        }

        return camera.aspect;
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("AspectRatioController는 Camera 컴포넌트가 필요합니다!");
            enabled = false;
            return;
        }

        targetAspectRatio = targetAspect.x / targetAspect.y;
        UpdateCameraRect();
    }

    void Update()
    {
        UpdateCameraRect();
    }

    void UpdateCameraRect()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspectRatio;

        if (scaleHeight < 1.0f)
        {
            // 레터박스 (상하 검은 막대) - 화면이 목표보다 좁음 (세로가 더 김)
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            // 필러박스 (좌우 검은 막대) - 화면이 목표보다 넓음 (가로가 더 김)
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }

    void OnGUI()
    {
        // 레터박스/필러박스 영역을 배경색으로 채우기
        if (cam == null) return;

        Rect cameraRect = cam.rect;

        // 왼쪽 필러박스
        if (cameraRect.x > 0)
        {
            GUI.color = letterboxColor;
            GUI.DrawTexture(new Rect(0, 0, cameraRect.x * Screen.width, Screen.height), Texture2D.whiteTexture);
        }

        // 오른쪽 필러박스
        float rightX = (cameraRect.x + cameraRect.width) * Screen.width;
        if (rightX < Screen.width)
        {
            GUI.color = letterboxColor;
            GUI.DrawTexture(new Rect(rightX, 0, Screen.width - rightX, Screen.height), Texture2D.whiteTexture);
        }

        // 위쪽 레터박스
        float topY = (cameraRect.y + cameraRect.height) * Screen.height;
        if (topY < Screen.height)
        {
            GUI.color = letterboxColor;
            GUI.DrawTexture(new Rect(0, topY, Screen.width, Screen.height - topY), Texture2D.whiteTexture);
        }

        // 아래쪽 레터박스
        if (cameraRect.y > 0)
        {
            GUI.color = letterboxColor;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, cameraRect.y * Screen.height), Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (targetAspect.x <= 0) targetAspect.x = 16;
        if (targetAspect.y <= 0) targetAspect.y = 9;
    }
#endif
}
