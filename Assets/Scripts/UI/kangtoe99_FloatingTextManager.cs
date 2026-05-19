using System.Collections;
using TMPro;
using UnityEngine;

// 월드 공간 떠오르는 텍스트 싱글톤 — 외부 자산 의존성 없음.
// AfterSceneLoad 자동 생성, DontDestroyOnLoad. 사용: kangtoe99_FloatingTextManager.Instance.Show(...).
public class kangtoe99_FloatingTextManager : MonoBehaviour
{
    public static kangtoe99_FloatingTextManager Instance { get; private set; }

    private Canvas worldCanvas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[FloatingTextManager]");
        go.AddComponent<kangtoe99_FloatingTextManager>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildCanvas()
    {
        var canvasGo = new GameObject("FloatingTextCanvas");
        canvasGo.transform.SetParent(transform, false);
        worldCanvas = canvasGo.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 100;

        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1f, 1f);
        rt.localScale = Vector3.one * 0.01f; // 월드 1유닛 = UI 100px 정도 매칭
    }

    public void Show(string text, Vector3 worldPos, Color color,
        float fontSize = 8f, float duration = 1.2f, float riseDistance = 2f)
    {
        if (worldCanvas == null) return;

        var go = new GameObject("FloatingText");
        go.transform.SetParent(worldCanvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.position = worldPos;
        rt.sizeDelta = new Vector2(400, 80);
        rt.localScale = Vector3.one;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = fontSize * 10f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        tmp.raycastTarget = false;

        StartCoroutine(Animate(go, rt, tmp, worldPos, duration, riseDistance, color));
    }

    // 플레이어 머리 위에 즉시 표시.
    public void ShowAtPlayer(string text, Color color,
        float yOffset = 0.5f, float fontSize = 4f, float duration = 1.0f, float riseDistance = 1.0f)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 basePos = player != null ? player.transform.position : Vector3.zero;
        Show(text, basePos + Vector3.up * yOffset, color, fontSize, duration, riseDistance);
    }

    private IEnumerator Animate(GameObject go, RectTransform rt, TextMeshProUGUI tmp,
        Vector3 startPos, float duration, float riseDistance, Color startColor)
    {
        float elapsed = 0f;
        while (elapsed < duration && go != null)
        {
            // unscaledDeltaTime — Time.timeScale=0 인 일시정지(레벨업 패널 등) 중에도 자연스럽게 진행.
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.position = startPos + Vector3.up * (riseDistance * t);
            Color c = startColor;
            c.a = (1f - t) * startColor.a;
            tmp.color = c;
            yield return null;
        }
        if (go != null) Destroy(go);
    }
}
