using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 씬에 단 하나 존재하는 아이템 호버 툴팁.
// ItemDisplayView 가 호버 시 Show(data, anchor), 떠날 때 Hide() 를 호출.
// 위치: 슬롯의 좌상단 코너 + offsetFromAnchor(픽셀) → 툴팁 pivot(0, 0) 이 거기 닿음 (= 슬롯 위로 좌측 정렬).
// 비활성 상태로 시작 가능 — 첫 호출 시 lazy find + 자동 활성화.
public class kangtoe99_ItemTooltip : MonoBehaviour
{
    private static kangtoe99_ItemTooltip _instance;
    public static kangtoe99_ItemTooltip Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<kangtoe99_ItemTooltip>(FindObjectsInactive.Include);
                if (_instance != null && !_instance.gameObject.activeSelf)
                    _instance.gameObject.SetActive(true); // 비활성으로 둔 경우 첫 사용 시 깨움
            }
            return _instance;
        }
    }

    [SerializeField] private GameObject root; // 토글할 패널 루트 (비우면 이 GameObject 자체)
    [SerializeField] private Image backgroundImage; // 등급 색으로 칠해짐 (알파는 기존 값 유지)
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private kangtoe99_TierColorPalette tierPalette;
    [SerializeField] private Vector2 offsetFromAnchor = new Vector2(0f, 0f); // 슬롯 좌상단 코너 + 픽셀

    private GameObject Target => root != null ? root : gameObject;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
        Hide();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public void Show(kangtoe99_ItemData data, RectTransform anchor)
    {
        if (data == null || anchor == null) return;
        if (nameText != null) nameText.text = data.DisplayName;
        if (descriptionText != null) descriptionText.text = data.Description;
        if (backgroundImage != null && tierPalette != null)
        {
            var c = tierPalette.Get(data.Tier);
            c.a = backgroundImage.color.a; // 기존 알파(투명도) 유지, RGB 만 등급 색으로
            backgroundImage.color = c;
        }
        Target.SetActive(true);
        transform.SetAsLastSibling(); // 최상단 렌더

        // 슬롯 상단 중앙 → 화면 좌표 + 픽셀 offset → 부모 캔버스 평면의 world 좌표
        var rt = (RectTransform)transform;
        var parentRt = rt.parent as RectTransform;
        if (parentRt == null) return;
        var canvas = GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera : null;

        var corners = new Vector3[4];
        anchor.GetWorldCorners(corners); // 0=BL, 1=TL, 2=TR, 3=BR
        Vector3 topLeft = corners[1];

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, topLeft);
        screen += offsetFromAnchor;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRt, screen, cam, out Vector3 world))
            transform.position = world;
    }

    public void Hide()
    {
        Target.SetActive(false);
    }
}
