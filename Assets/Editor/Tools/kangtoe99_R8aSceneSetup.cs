using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// R8a + Build UI 자동 셋업 도구.
// 메뉴: Tools > BumpOrBlast > Setup Scene UI (R8a + Build)
// 시각 prefab은 단일 공통 ItemDisplay.prefab — 슬롯이 인스턴스화 후 ItemDisplayView.Bind로 SO 데이터 주입.
// 모든 단계 idempotent. 슬롯 prefab은 displayContainer 필드 누락(v1/v2) 감지 시 재생성.
public static class kangtoe99_R8aSceneSetup
{
    private const string ChoiceSlotPrefabPath = "Assets/Prefabs/UIs/LevelUpChoiceSlot.prefab";
    private const string BuildEntrySlotPrefabPath = "Assets/Prefabs/UIs/BuildEntrySlot.prefab";
    private const string ItemDisplayPrefabPath = "Assets/Prefabs/UIs/ItemDisplay.prefab";

    [MenuItem("Tools/BumpOrBlast/Setup Scene UI (R8a + Build)")]
    public static void Setup()
    {
        kangtoe99_SampleItemDataCreator.CreateSamples();

        var player = FindInScene<kangtoe99_Player>();
        if (player == null) { Debug.LogError("[R8aSetup] kangtoe99_Player 미발견 — Scene이 열려있는지 확인"); return; }
        if (player.GetComponent<kangtoe99_ItemInventory>() == null)
        {
            Undo.AddComponent<kangtoe99_ItemInventory>(player.gameObject);
            Debug.Log("[R8aSetup] PlayerCharacter에 ItemInventory 부착");
        }

        var itemDisplay = EnsureItemDisplayPrefab();
        if (itemDisplay == null) return;

        var choiceSlotPrefab = EnsureChoiceSlotPrefab(itemDisplay);
        if (choiceSlotPrefab == null) return;

        var levelUp = FindInScene<kangtoe99_LevelUpSystem>();
        if (levelUp == null) { Debug.LogError("[R8aSetup] kangtoe99_LevelUpSystem 미발견"); return; }
        WireLevelUpSystem(levelUp, player, choiceSlotPrefab);

        var buildSlotPrefab = EnsureBuildEntrySlotPrefab(itemDisplay);
        if (buildSlotPrefab == null) return;

        var gameOverUI = FindInScene<kangtoe99_GameOverUI>();
        var allCanvases = FindAllInScene<Canvas>();
        Canvas canvas = null;
        if (gameOverUI != null) canvas = gameOverUI.GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            foreach (var c in allCanvases) { if (c.isRootCanvas) { canvas = c; break; } }
            if (canvas == null && allCanvases.Count > 0) canvas = allCanvases[0];
        }
        Debug.Log($"[R8aSetup] Canvas 검색: 후보 {allCanvases.Count}개 → 선택 {(canvas != null ? canvas.name : "null")}");
        if (canvas == null) { Debug.LogError("[R8aSetup] Canvas 미발견"); return; }

        EnsureHudBuildArea(canvas, player.GetComponent<kangtoe99_ItemInventory>(), buildSlotPrefab);
        EnsurePauseSystem(canvas, player.GetComponent<kangtoe99_ItemInventory>(), buildSlotPrefab);
        if (gameOverUI != null)
        {
            EnsureGameOverBuildArea(gameOverUI, player.GetComponent<kangtoe99_ItemInventory>(), buildSlotPrefab);
        }

        EditorSceneManager.MarkSceneDirty(levelUp.gameObject.scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[R8aSetup] 완료 — Scene 저장하세요 (Ctrl+S)");
    }

    // ─────────────────────────────────────────────────────────────
    // 공통 ItemDisplay prefab — 슬롯이 자식으로 인스턴스화
    // ─────────────────────────────────────────────────────────────

    private static kangtoe99_ItemDisplayView EnsureItemDisplayPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<kangtoe99_ItemDisplayView>(ItemDisplayPrefabPath);
        if (existing != null) return existing;

        EnsureFolder("Assets/Prefabs/UIs");

        var root = new GameObject("ItemDisplay",
            typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement),
            typeof(kangtoe99_ItemDisplayView));
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 200);

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        var le = root.GetComponent<LayoutElement>();
        le.preferredWidth = 180;
        le.preferredHeight = 200;

        // IconHolder (icon + stack 오버레이)
        var iconHolder = new GameObject("IconHolder", typeof(RectTransform), typeof(LayoutElement));
        iconHolder.transform.SetParent(root.transform, false);
        iconHolder.GetComponent<LayoutElement>().preferredHeight = 96;
        var iconHolderRt = iconHolder.GetComponent<RectTransform>();
        iconHolderRt.sizeDelta = new Vector2(96, 96);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(iconHolder.transform, false);
        StretchFill(iconGo);
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.enabled = false;

        // Stack — 우상단 anchor
        var stackGo = new GameObject("Stack", typeof(RectTransform), typeof(Text));
        stackGo.transform.SetParent(iconHolder.transform, false);
        var stackRt = stackGo.GetComponent<RectTransform>();
        stackRt.anchorMin = new Vector2(1f, 1f);
        stackRt.anchorMax = new Vector2(1f, 1f);
        stackRt.pivot = new Vector2(1f, 1f);
        stackRt.anchoredPosition = new Vector2(-2, -2);
        stackRt.sizeDelta = new Vector2(40, 20);
        var stackText = stackGo.GetComponent<Text>();
        stackText.font = GetBuiltinFont();
        stackText.fontSize = 14;
        stackText.fontStyle = FontStyle.Bold;
        stackText.alignment = TextAnchor.MiddleRight;
        stackText.color = Color.yellow;
        stackText.text = string.Empty;
        stackText.horizontalOverflow = HorizontalWrapMode.Overflow;
        stackText.verticalOverflow = VerticalWrapMode.Overflow;

        var nameText = AddChildText(root.transform, "Name", 24, 14, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        var descText = AddChildText(root.transform, "Description", 60, 11, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f), TextAnchor.UpperCenter, wrap: true);

        var view = root.GetComponent<kangtoe99_ItemDisplayView>();
        var so = new SerializedObject(view);
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("stackText").objectReferenceValue = stackText;
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("descriptionText").objectReferenceValue = descText;
        so.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(root, ItemDisplayPrefabPath, out bool ok);
        Object.DestroyImmediate(root);
        if (!ok) { Debug.LogError("[R8aSetup] ItemDisplay prefab 저장 실패"); return null; }
        Debug.Log($"[R8aSetup] 공통 prefab 생성: {ItemDisplayPrefabPath}");
        return saved.GetComponent<kangtoe99_ItemDisplayView>();
    }

    // ─────────────────────────────────────────────────────────────
    // LevelUp Choice Slot prefab (v3 — displayContainer + displayPrefab)
    // ─────────────────────────────────────────────────────────────

    private static kangtoe99_LevelUpChoiceSlot EnsureChoiceSlotPrefab(kangtoe99_ItemDisplayView itemDisplayPrefab)
    {
        var existing = AssetDatabase.LoadAssetAtPath<kangtoe99_LevelUpChoiceSlot>(ChoiceSlotPrefabPath);
        if (existing != null)
        {
            var soCheck = new SerializedObject(existing);
            var dcProp = soCheck.FindProperty("displayContainer");
            var dpProp = soCheck.FindProperty("displayPrefab");
            if (dcProp != null && dcProp.objectReferenceValue != null
                && dpProp != null && dpProp.objectReferenceValue != null)
            {
                return existing;
            }
            AssetDatabase.DeleteAsset(ChoiceSlotPrefabPath);
            Debug.LogWarning($"[R8aSetup] 기존 구버전 LevelUpChoiceSlot 삭제 후 재생성");
        }

        EnsureFolder("Assets/Prefabs/UIs");
        var root = new GameObject("LevelUpChoiceSlot",
            typeof(RectTransform), typeof(Image), typeof(Button),
            typeof(VerticalLayoutGroup), typeof(LayoutElement),
            typeof(kangtoe99_LevelUpChoiceSlot));
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 240);
        root.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var le = root.GetComponent<LayoutElement>();
        le.preferredWidth = 220;
        le.preferredHeight = 240;

        // DisplayContainer — ItemDisplay 인스턴스화 자리
        var dcGo = new GameObject("DisplayContainer", typeof(RectTransform), typeof(LayoutElement));
        dcGo.transform.SetParent(root.transform, false);
        dcGo.GetComponent<LayoutElement>().preferredHeight = 200;
        var dcRt = dcGo.GetComponent<RectTransform>();
        dcRt.sizeDelta = new Vector2(200, 200);

        var slot = root.GetComponent<kangtoe99_LevelUpChoiceSlot>();
        var so = new SerializedObject(slot);
        so.FindProperty("button").objectReferenceValue = root.GetComponent<Button>();
        so.FindProperty("displayContainer").objectReferenceValue = dcRt;
        so.FindProperty("displayPrefab").objectReferenceValue = itemDisplayPrefab;
        so.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(root, ChoiceSlotPrefabPath, out bool ok);
        Object.DestroyImmediate(root);
        if (!ok) { Debug.LogError("[R8aSetup] LevelUpChoiceSlot prefab 저장 실패"); return null; }
        Debug.Log($"[R8aSetup] prefab 생성: {ChoiceSlotPrefabPath}");
        return saved.GetComponent<kangtoe99_LevelUpChoiceSlot>();
    }

    // ─────────────────────────────────────────────────────────────
    // LevelUpSystem 배선
    // ─────────────────────────────────────────────────────────────

    private static void WireLevelUpSystem(kangtoe99_LevelUpSystem levelUp, kangtoe99_Player player, kangtoe99_LevelUpChoiceSlot slotPrefab)
    {
        Undo.RecordObject(levelUp, "Wire LevelUpSystem");
        var so = new SerializedObject(levelUp);
        so.FindProperty("player").objectReferenceValue = player;

        var itemPoolProp = so.FindProperty("itemPool");
        var items = FindAllAssets<kangtoe99_ItemData>();
        itemPoolProp.arraySize = items.Count;
        for (int i = 0; i < items.Count; i++) itemPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = items[i];

        var dropPoolProp = so.FindProperty("instantDropPool");
        var drops = FindAllAssets<kangtoe99_InstantDropItemData>();
        dropPoolProp.arraySize = drops.Count;
        for (int i = 0; i < drops.Count; i++) dropPoolProp.GetArrayElementAtIndex(i).objectReferenceValue = drops[i];

        so.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;

        var panelProp = so.FindProperty("levelUpPanel");
        var panel = panelProp.objectReferenceValue as GameObject;
        if (panel != null)
        {
            var container = EnsureLevelUpChoiceContainer(panel);
            so.FindProperty("slotContainer").objectReferenceValue = container.transform;
        }
        else
        {
            Debug.LogWarning("[R8aSetup] LevelUpSystem.levelUpPanel 미할당 — slotContainer 자동 생성 스킵");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(levelUp);
        Debug.Log($"[R8aSetup] LevelUpSystem 배선 (itemPool={items.Count}, instantDropPool={drops.Count})");
    }

    private static GameObject EnsureLevelUpChoiceContainer(GameObject panel)
    {
        var existing = panel.transform.Find("ChoiceContainer");
        if (existing != null) return existing.gameObject;

        var container = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        Undo.RegisterCreatedObjectUndo(container, "Create ChoiceContainer");
        container.transform.SetParent(panel.transform, false);
        StretchInside(container, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.8f));
        var layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 16;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return container;
    }

    // ─────────────────────────────────────────────────────────────
    // BuildEntrySlot prefab (v3 — displayContainer + displayPrefab)
    // ─────────────────────────────────────────────────────────────

    private static kangtoe99_BuildEntrySlot EnsureBuildEntrySlotPrefab(kangtoe99_ItemDisplayView itemDisplayPrefab)
    {
        var existing = AssetDatabase.LoadAssetAtPath<kangtoe99_BuildEntrySlot>(BuildEntrySlotPrefabPath);
        if (existing != null)
        {
            var soCheck = new SerializedObject(existing);
            var dcProp = soCheck.FindProperty("displayContainer");
            var dpProp = soCheck.FindProperty("displayPrefab");
            if (dcProp != null && dcProp.objectReferenceValue != null
                && dpProp != null && dpProp.objectReferenceValue != null)
            {
                return existing;
            }
            AssetDatabase.DeleteAsset(BuildEntrySlotPrefabPath);
            Debug.LogWarning($"[R8aSetup] 기존 구버전 BuildEntrySlot 삭제 후 재생성");
        }

        EnsureFolder("Assets/Prefabs/UIs");
        var root = new GameObject("BuildEntrySlot",
            typeof(RectTransform), typeof(Image), typeof(LayoutElement),
            typeof(kangtoe99_BuildEntrySlot));
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(72, 96);
        root.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.6f);

        var le = root.GetComponent<LayoutElement>();
        le.preferredWidth = 72;
        le.preferredHeight = 96;

        // DisplayContainer — ItemDisplay 인스턴스화 자리
        var dcGo = new GameObject("DisplayContainer", typeof(RectTransform));
        dcGo.transform.SetParent(root.transform, false);
        StretchFill(dcGo);

        var slot = root.GetComponent<kangtoe99_BuildEntrySlot>();
        var so = new SerializedObject(slot);
        so.FindProperty("displayContainer").objectReferenceValue = dcGo.GetComponent<RectTransform>();
        so.FindProperty("displayPrefab").objectReferenceValue = itemDisplayPrefab;
        so.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(root, BuildEntrySlotPrefabPath, out bool ok);
        Object.DestroyImmediate(root);
        if (!ok) { Debug.LogError("[R8aSetup] BuildEntrySlot prefab 저장 실패"); return null; }
        Debug.Log($"[R8aSetup] prefab 생성: {BuildEntrySlotPrefabPath}");
        return saved.GetComponent<kangtoe99_BuildEntrySlot>();
    }

    // ─────────────────────────────────────────────────────────────
    // HUD Build 영역
    // ─────────────────────────────────────────────────────────────

    private static void EnsureHudBuildArea(Canvas canvas, kangtoe99_ItemInventory inventory, kangtoe99_BuildEntrySlot slotPrefab)
    {
        var existing = canvas.transform.Find("HUD_BuildContainer");
        if (existing != null) return;

        var go = new GameObject("HUD_BuildContainer",
            typeof(RectTransform), typeof(HorizontalLayoutGroup),
            typeof(kangtoe99_BuildDisplayUI));
        Undo.RegisterCreatedObjectUndo(go, "Create HUD_BuildContainer");
        go.transform.SetParent(canvas.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(16, -16);
        rt.sizeDelta = new Vector2(600, 100);

        var layout = go.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 6;
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        var display = go.GetComponent<kangtoe99_BuildDisplayUI>();
        var so = new SerializedObject(display);
        so.FindProperty("inventory").objectReferenceValue = inventory;
        so.FindProperty("slotContainer").objectReferenceValue = go.transform;
        so.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        so.FindProperty("showName").boolValue = false;
        so.FindProperty("showDescription").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[R8aSetup] HUD 빌드 영역 생성");
    }

    // ─────────────────────────────────────────────────────────────
    // Pause UI
    // ─────────────────────────────────────────────────────────────

    private static void EnsurePauseSystem(Canvas canvas, kangtoe99_ItemInventory inventory, kangtoe99_BuildEntrySlot slotPrefab)
    {
        var existing = FindInScene<kangtoe99_PauseSystem>();
        GameObject pausePanel;
        if (existing != null)
        {
            var soP = new SerializedObject(existing);
            pausePanel = soP.FindProperty("pausePanel").objectReferenceValue as GameObject;
            if (pausePanel == null)
            {
                pausePanel = CreatePausePanel(canvas, inventory, slotPrefab);
                soP.FindProperty("pausePanel").objectReferenceValue = pausePanel;
                soP.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(existing);
            }
            return;
        }

        pausePanel = CreatePausePanel(canvas, inventory, slotPrefab);
        var pauseGo = new GameObject("PauseSystem", typeof(kangtoe99_PauseSystem));
        Undo.RegisterCreatedObjectUndo(pauseGo, "Create PauseSystem");
        var system = pauseGo.GetComponent<kangtoe99_PauseSystem>();
        var so = new SerializedObject(system);
        so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[R8aSetup] PauseSystem + PausePanel 생성");
    }

    private static GameObject CreatePausePanel(Canvas canvas, kangtoe99_ItemInventory inventory, kangtoe99_BuildEntrySlot slotPrefab)
    {
        var panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(panel, "Create PausePanel");
        panel.transform.SetParent(canvas.transform, false);
        StretchInside(panel, Vector2.zero, Vector2.one);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(panel.transform, false);
        var title = titleGo.GetComponent<Text>();
        title.font = GetBuiltinFont();
        title.fontSize = 40;
        title.fontStyle = FontStyle.Bold;
        title.color = Color.white;
        title.alignment = TextAnchor.MiddleCenter;
        title.text = "Paused";
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -60);
        titleRt.sizeDelta = new Vector2(400, 60);

        var hintGo = new GameObject("ResumeHint", typeof(RectTransform), typeof(Text));
        hintGo.transform.SetParent(panel.transform, false);
        var hint = hintGo.GetComponent<Text>();
        hint.font = GetBuiltinFont();
        hint.fontSize = 18;
        hint.color = new Color(0.85f, 0.85f, 0.85f);
        hint.alignment = TextAnchor.MiddleCenter;
        hint.text = "Press ESC to Resume";
        var hintRt = hintGo.GetComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(0.5f, 0f);
        hintRt.anchorMax = new Vector2(0.5f, 0f);
        hintRt.pivot = new Vector2(0.5f, 0f);
        hintRt.anchoredPosition = new Vector2(0, 60);
        hintRt.sizeDelta = new Vector2(400, 40);

        var buildContainer = new GameObject("BuildContainer",
            typeof(RectTransform), typeof(GridLayoutGroup),
            typeof(kangtoe99_BuildDisplayUI));
        buildContainer.transform.SetParent(panel.transform, false);
        StretchInside(buildContainer, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.75f));
        var grid = buildContainer.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(100, 130);
        grid.spacing = new Vector2(8, 8);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;

        var display = buildContainer.GetComponent<kangtoe99_BuildDisplayUI>();
        var soD = new SerializedObject(display);
        soD.FindProperty("inventory").objectReferenceValue = inventory;
        soD.FindProperty("slotContainer").objectReferenceValue = buildContainer.transform;
        soD.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        soD.FindProperty("showName").boolValue = true;
        soD.FindProperty("showDescription").boolValue = false;
        soD.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
        return panel;
    }

    // ─────────────────────────────────────────────────────────────
    // GameOver Build 영역
    // ─────────────────────────────────────────────────────────────

    private static void EnsureGameOverBuildArea(kangtoe99_GameOverUI gameOverUI, kangtoe99_ItemInventory inventory, kangtoe99_BuildEntrySlot slotPrefab)
    {
        var soG = new SerializedObject(gameOverUI);
        var existingProp = soG.FindProperty("buildDisplay");
        var display = existingProp.objectReferenceValue as kangtoe99_BuildDisplayUI;
        if (display != null) return;

        Transform panelTr = gameOverUI.transform;
        var existingContainer = panelTr.Find("GameOver_BuildContainer");
        GameObject container;
        if (existingContainer != null)
        {
            container = existingContainer.gameObject;
        }
        else
        {
            container = new GameObject("GameOver_BuildContainer",
                typeof(RectTransform), typeof(HorizontalLayoutGroup),
                typeof(kangtoe99_BuildDisplayUI));
            Undo.RegisterCreatedObjectUndo(container, "Create GameOver BuildContainer");
            container.transform.SetParent(panelTr, false);
            StretchInside(container, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.18f));
            var layout = container.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
        }

        display = container.GetComponent<kangtoe99_BuildDisplayUI>();
        var soD = new SerializedObject(display);
        soD.FindProperty("inventory").objectReferenceValue = inventory;
        soD.FindProperty("slotContainer").objectReferenceValue = container.transform;
        soD.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        soD.FindProperty("showName").boolValue = false;
        soD.FindProperty("showDescription").boolValue = false;
        soD.ApplyModifiedPropertiesWithoutUndo();

        existingProp.objectReferenceValue = display;
        soG.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(gameOverUI);
        Debug.Log("[R8aSetup] GameOverUI 빌드 영역 + buildDisplay 연결");
    }

    // ─────────────────────────────────────────────────────────────
    // 유틸
    // ─────────────────────────────────────────────────────────────

    private static Text AddChildText(Transform parent, string name, float preferredHeight, int fontSize, FontStyle style, Color color, TextAnchor anchor, bool wrap = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = GetBuiltinFont();
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = anchor;
        t.color = color;
        t.text = name;
        t.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        go.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
        return t;
    }

    private static void StretchFill(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchInside(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static T FindInScene<T>() where T : Component
    {
        var all = Resources.FindObjectsOfTypeAll<T>();
        foreach (var item in all)
        {
            if (item == null) continue;
            if (!item.gameObject.scene.IsValid()) continue;
            return item;
        }
        return null;
    }

    private static List<T> FindAllInScene<T>() where T : Component
    {
        var all = Resources.FindObjectsOfTypeAll<T>();
        var result = new List<T>();
        foreach (var item in all)
        {
            if (item == null) continue;
            if (!item.gameObject.scene.IsValid()) continue;
            result.Add(item);
        }
        return result;
    }

    private static List<T> FindAllAssets<T>() where T : Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var result = new List<T>(guids.Length);
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) result.Add(asset);
        }
        return result;
    }

    private static Font GetBuiltinFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;
        var parts = folderPath.Split('/');
        string accum = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{accum}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(accum, parts[i]);
            accum = next;
        }
    }
}
