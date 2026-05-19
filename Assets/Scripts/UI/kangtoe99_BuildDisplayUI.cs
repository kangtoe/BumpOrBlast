using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 한 영역(HUD / Pause / GameOver)에서 ItemInventory 의 빌드를 페이지 단위로 표시.
// 한 페이지에 cols × rows 개 슬롯. 페이지 이동은 마우스 휠 또는 도트 클릭.
// 페이지가 1개 이하면 도트는 생성되지 않음.
// 도트는 BuildContainer (this 스크립트의 GameObject) 의 자식으로 직접 생성되며,
// 위치는 BuildContainer pivot 기준 dotOffset + 가운데 정렬.
public class kangtoe99_BuildDisplayUI : MonoBehaviour, IScrollHandler
{
    [Header("Data / Slot")]
    [SerializeField] private kangtoe99_ItemInventory inventory;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private kangtoe99_ItemDisplayView slotPrefab;

    [Header("Pagination")]
    [SerializeField, Min(1)] private int columns = 2;
    [SerializeField, Min(1)] private int rows = 4;
    [SerializeField] private Vector2 cellSpacing = new Vector2(8, 8);
    [SerializeField] private Vector2 cellPadding = new Vector2(8, 8); // 좌상단 패딩 (x=left, y=top)
    [SerializeField] private Vector2 dotSize = new Vector2(10, 10);
    [SerializeField, Min(0f)] private float dotSpacing = 8f;
    [SerializeField] private Vector2 dotOffset = new Vector2(0f, -180f); // BuildContainer pivot 기준 오프셋
    [SerializeField] private Color dotActiveColor = Color.white;
    [SerializeField] private Color dotInactiveColor = new Color(1, 1, 1, 0.35f);
    [SerializeField, Min(0f)] private float wheelCooldown = 0.12f; // 터치패드 smooth scroll 떨림 방지

    [Header("Page Buttons (optional)")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField, Min(0f)] private float buttonGap = 8f; // 도트 row 끝과 버튼 사이 간격

    [Header("Empty State")]
    [SerializeField] private GameObject emptyStateLabel; // 빌드가 비었을 때만 표시

    [Header("Gizmo (Editor only)")]
    [SerializeField, Min(1)] private int previewPages = 4;

    private readonly List<kangtoe99_ItemDisplayView> activeSlots = new List<kangtoe99_ItemDisplayView>();
    private readonly Stack<kangtoe99_ItemDisplayView> pooledSlots = new Stack<kangtoe99_ItemDisplayView>();
    private readonly List<Button> activeDots = new List<Button>();
    private readonly List<kangtoe99_ItemInventory.BuildEntry> entryBuffer = new List<kangtoe99_ItemInventory.BuildEntry>();
    private int currentPage;
    private bool subscribed;
    private float lastWheelTime;

    public int Columns => Mathf.Max(1, columns);
    public int Rows => Mathf.Max(1, rows);
    private int PageSize => Columns * Rows;

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
    }

    private void Start() { EnsureInventory(); Subscribe(); Refresh(); }

    private void OnEnable() { EnsureInventory(); Subscribe(); Refresh(); }

    private void OnDisable() { Unsubscribe(); }

    private void OnDestroy()
    {
        Unsubscribe();
        if (prevButton != null) prevButton.onClick.RemoveListener(PrevPage);
        if (nextButton != null) nextButton.onClick.RemoveListener(NextPage);
    }

    public void PrevPage() => SetPage(currentPage - 1);
    public void NextPage() => SetPage(currentPage + 1);

    private void EnsureInventory()
    {
        if (inventory == null) inventory = FindFirstObjectByType<kangtoe99_ItemInventory>();
    }

    private void Subscribe()
    {
        if (subscribed || inventory == null) return;
        inventory.OnItemAdded += OnItemAdded;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || inventory == null) return;
        inventory.OnItemAdded -= OnItemAdded;
        subscribed = false;
    }

    // 새 아이템 추가 시 마지막 페이지로 이동 — 새 빌드를 바로 확인.
    private void OnItemAdded(kangtoe99_ItemData data, int stack)
    {
        currentPage = int.MaxValue;
        Refresh();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (Time.unscaledTime - lastWheelTime < wheelCooldown) return;
        float dy = eventData.scrollDelta.y;
        if (dy > 0.01f) { SetPage(currentPage - 1); lastWheelTime = Time.unscaledTime; }
        else if (dy < -0.01f) { SetPage(currentPage + 1); lastWheelTime = Time.unscaledTime; }
    }

    public void SetPage(int page)
    {
        currentPage = Mathf.Clamp(page, 0, LastPageIndex());
        Refresh();
    }

    private int LastPageIndex()
    {
        if (inventory == null) return 0;
        int count = inventory.EntryCount;
        if (count <= 0) return 0;
        return (count - 1) / PageSize;
    }

    public void Refresh()
    {
        if (slotContainer == null || slotPrefab == null) return;

        // 활성 슬롯들을 풀로 반환 — Destroy/Instantiate 사이클 회피.
        for (int i = activeSlots.Count - 1; i >= 0; i--)
        {
            var s = activeSlots[i];
            if (s == null) continue;
            s.gameObject.SetActive(false);
            pooledSlots.Push(s);
        }
        activeSlots.Clear();

        if (inventory == null)
        {
            UpdateDots(0);
            if (emptyStateLabel != null) emptyStateLabel.SetActive(true);
            return;
        }

        entryBuffer.Clear();
        foreach (var entry in inventory.GetBuildEntries()) entryBuffer.Add(entry);
        int total = entryBuffer.Count;

        int last = total > 0 ? (total - 1) / PageSize : 0;
        currentPage = Mathf.Clamp(currentPage, 0, last);

        int start = currentPage * PageSize;
        int end = Mathf.Min(start + PageSize, total);
        for (int i = start; i < end; i++)
        {
            var slot = AcquireSlot();
            slot.Bind(entryBuffer[i].data, entryBuffer[i].stack);
            activeSlots.Add(slot);
        }
        entryBuffer.Clear();

        LayoutSlots();
        UpdateDots(total);

        if (emptyStateLabel != null) emptyStateLabel.SetActive(total == 0);
    }

    // 씬 시작 시 미리 호출하면 슬롯 prefab Instantiate 비용을 로딩 구간에 지불 — 게임오버/일시정지
    // 패널 첫 활성화 시 발생하는 hitch 를 줄인다. 풀에는 비활성 상태로 보관.
    public void WarmUp(int count)
    {
        if (slotContainer == null || slotPrefab == null) return;
        count = Mathf.Max(0, count);
        while (pooledSlots.Count + activeSlots.Count < count)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.gameObject.SetActive(false);
            pooledSlots.Push(slot);
        }
    }

    private kangtoe99_ItemDisplayView AcquireSlot()
    {
        kangtoe99_ItemDisplayView slot;
        while (pooledSlots.Count > 0)
        {
            slot = pooledSlots.Pop();
            if (slot == null) continue; // 씬 리로드 등으로 죽은 참조 스킵
            slot.gameObject.SetActive(true);
            return slot;
        }
        slot = Instantiate(slotPrefab, slotContainer);
        return slot;
    }

    // GridLayoutGroup 대신 수동 배치 — 슬롯 prefab 사이즈 그대로 존중.
    // 모든 슬롯이 동일 prefab 이라 첫 슬롯 사이즈를 기준으로 배열하고,
    // 컨테이너도 cols/rows/padding/spacing 으로 결정되는 정확한 크기로 self-resize.
    private void LayoutSlots()
    {
        var sc = slotContainer as RectTransform;

        Vector2 cell = GetCellSize();
        if (cell.x <= 0f || cell.y <= 0f) return;

        // 컨테이너 사이즈 = padding(양쪽) + cols*cell + (cols-1)*spacing  (rows 동일)
        if (sc != null)
        {
            float w = cellPadding.x * 2 + Columns * cell.x + Mathf.Max(0, Columns - 1) * cellSpacing.x;
            float h = cellPadding.y * 2 + Rows * cell.y + Mathf.Max(0, Rows - 1) * cellSpacing.y;
            sc.sizeDelta = new Vector2(w, h);
        }

        int count = activeSlots.Count;
        int totalRows = (count + Columns - 1) / Columns;
        float containerW = cellPadding.x * 2 + Columns * cell.x + Mathf.Max(0, Columns - 1) * cellSpacing.x;

        // 한 줄이면 가운데 정렬, 두 줄 이상이면 좌측 정렬(cellPadding 기준)
        float singleRowStartX = 0f;
        if (totalRows <= 1 && count > 0)
        {
            float rowWidth = count * cell.x + Mathf.Max(0, count - 1) * cellSpacing.x;
            singleRowStartX = (containerW - rowWidth) * 0.5f;
        }

        for (int i = 0; i < count; i++)
        {
            int row = i / Columns;
            int col = i % Columns;

            var rt = (RectTransform)activeSlots[i].transform;
            // 좌상단 기준 anchor/pivot 강제 — prefab 의 anchor 와 무관하게 일관된 배치
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            float baseX = totalRows <= 1 ? singleRowStartX : cellPadding.x;
            float x = baseX + col * (cell.x + cellSpacing.x);
            float y = -(cellPadding.y + row * (cell.y + cellSpacing.y));
            rt.anchoredPosition = new Vector2(x, y);
        }
    }

    // 첫 활성 슬롯 → 없으면 prefab 의 RectTransform 사이즈로 폴백.
    // (슬롯이 0개여도 컨테이너 크기는 정해져야 함 — 빈 빌드도 영역 유지)
    private Vector2 GetCellSize()
    {
        if (activeSlots.Count > 0 && activeSlots[0] != null)
        {
            var rt = (RectTransform)activeSlots[0].transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            return rt.rect.size;
        }
        if (slotPrefab != null)
        {
            var prefabRt = slotPrefab.GetComponent<RectTransform>();
            if (prefabRt != null) return prefabRt.rect.size;
        }
        return Vector2.zero;
    }

#if UNITY_EDITOR
    // 사전 계산된 레이아웃 외곽 + 개별 셀을 Scene 뷰에 시각화.
    // 선택했을 때만 그림 — 일반 작업 중엔 방해되지 않도록.
    private void OnDrawGizmosSelected()
    {
        var sc = slotContainer as RectTransform;
        if (sc == null) return;

        Vector2 cell = GetCellSize();
        if (cell.x <= 0f || cell.y <= 0f) return;

        float w = cellPadding.x * 2 + Columns * cell.x + Mathf.Max(0, Columns - 1) * cellSpacing.x;
        float h = cellPadding.y * 2 + Rows * cell.y + Mathf.Max(0, Rows - 1) * cellSpacing.y;

        // 컨테이너 로컬 좌표의 좌상단 (pivot 보정)
        float originX = -sc.pivot.x * w;
        float originY = (1f - sc.pivot.y) * h;

        // 외곽
        DrawLocalRect(sc, originX, originY - h, w, h, new Color(0f, 1f, 1f, 0.9f));

        // 개별 셀
        var cellColor = new Color(1f, 0.85f, 0f, 0.6f);
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                float x0 = originX + cellPadding.x + c * (cell.x + cellSpacing.x);
                float yTop = originY - cellPadding.y - r * (cell.y + cellSpacing.y);
                DrawLocalRect(sc, x0, yTop - cell.y, cell.x, cell.y, cellColor);
            }
        }

        // 도트 미리보기 — BuildContainer 좌표계에서 offset + 가운데 정렬
        DrawDotsGizmo();
    }

    private void DrawDotsGizmo()
    {
        var sc = (RectTransform)transform;
        int n = Mathf.Max(1, previewPages);
        var dotColor = new Color(1f, 1f, 1f, 0.7f);

        for (int i = 0; i < n; i++)
        {
            float cx = (i - (n - 1) * 0.5f) * (dotSize.x + dotSpacing) + dotOffset.x;
            float cy = dotOffset.y;
            DrawLocalRect(sc, cx - dotSize.x * 0.5f, cy - dotSize.y * 0.5f, dotSize.x, dotSize.y, dotColor);
        }
    }

    private static void DrawLocalRect(Transform t, float x, float y, float w, float h, Color color)
    {
        var bl = t.TransformPoint(new Vector3(x, y, 0));
        var br = t.TransformPoint(new Vector3(x + w, y, 0));
        var tl = t.TransformPoint(new Vector3(x, y + h, 0));
        var tr = t.TransformPoint(new Vector3(x + w, y + h, 0));
        Gizmos.color = color;
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
#endif

    private void UpdateDots(int total)
    {
        int pageCount = total > 0 ? ((total - 1) / PageSize) + 1 : 0;

        // 이전/다음 버튼 — 페이지 2개 이상일 때만 표시, 끝 페이지에서는 interactable=false
        bool showPageNav = pageCount > 1;
        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(showPageNav);
            prevButton.interactable = currentPage > 0;
        }
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(showPageNav);
            nextButton.interactable = currentPage < pageCount - 1;
        }

        // 버튼 위치 — 도트 row 양 끝 + gap. 페이지 수에 따라 동적.
        if (pageCount > 1)
        {
            float dotRowW = pageCount * dotSize.x + Mathf.Max(0, pageCount - 1) * dotSpacing;
            float halfDots = dotRowW * 0.5f;
            PositionEdgeButton(prevButton, -halfDots - buttonGap, leftSide: true);
            PositionEdgeButton(nextButton, halfDots + buttonGap, leftSide: false);
        }

        // 풀 크기 조정 — BuildContainer (this script 의 GameObject) 자식으로 직접 생성
        while (activeDots.Count < pageCount)
        {
            int capturedIndex = activeDots.Count;
            var go = new GameObject("Dot", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = dotSize;
            var img = go.GetComponent<Image>();
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => SetPage(capturedIndex));
            activeDots.Add(btn);
        }
        while (activeDots.Count > pageCount)
        {
            int idx = activeDots.Count - 1;
            if (activeDots[idx] != null) Destroy(activeDots[idx].gameObject);
            activeDots.RemoveAt(idx);
        }

        // 배치 — BuildContainer pivot 기준 dotOffset + 가운데 정렬
        for (int i = 0; i < activeDots.Count; i++)
        {
            if (activeDots[i] == null) continue;
            var img = activeDots[i].targetGraphic as Image;
            if (img != null) img.color = i == currentPage ? dotActiveColor : dotInactiveColor;

            var rt = (RectTransform)activeDots[i].transform;
            float x = (i - (activeDots.Count - 1) * 0.5f) * (dotSize.x + dotSpacing) + dotOffset.x;
            float y = dotOffset.y;
            rt.anchoredPosition = new Vector2(x, y);
        }
    }

    // xRelative: 도트 row 중심(=dotOffset.x) 기준 위치. leftSide=true 면 그 위치가 버튼 오른쪽 가장자리,
    // false 면 왼쪽 가장자리에 닿도록 버튼 중심 좌표 계산.
    private void PositionEdgeButton(Button btn, float xRelative, bool leftSide)
    {
        if (btn == null) return;
        var rt = (RectTransform)btn.transform;
        float halfBtn = rt.rect.width * 0.5f;
        float cx = dotOffset.x + xRelative + (leftSide ? -halfBtn : halfBtn);
        rt.anchoredPosition = new Vector2(cx, dotOffset.y);
    }
}
