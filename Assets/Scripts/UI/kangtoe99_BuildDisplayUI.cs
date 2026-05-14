using System.Collections.Generic;
using UnityEngine;

// 한 영역(HUD / Pause / GameOver)에서 ItemInventory의 빌드를 표시.
// 아이템 UI(ItemDisplayView) prefab을 그대로 슬롯으로 사용 — 별도 BuildEntrySlot 중간 레이어 없음.
// inventory의 OnItemAdded 이벤트를 구독해 자동 갱신.
public class kangtoe99_BuildDisplayUI : MonoBehaviour
{
    [SerializeField] private kangtoe99_ItemInventory inventory;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private kangtoe99_ItemDisplayView slotPrefab;

    private readonly List<kangtoe99_ItemDisplayView> activeSlots = new List<kangtoe99_ItemDisplayView>();
    private bool subscribed;

    private void Start()
    {
        EnsureInventory();
        Subscribe();
        Refresh();
    }

    private void OnEnable()
    {
        EnsureInventory();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

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

    private void OnItemAdded(kangtoe99_ItemData data, int stack) => Refresh();

    public void Refresh()
    {
        if (slotContainer == null || slotPrefab == null) return;

        for (int i = activeSlots.Count - 1; i >= 0; i--)
        {
            if (activeSlots[i] != null) Destroy(activeSlots[i].gameObject);
        }
        activeSlots.Clear();

        if (inventory == null) return;

        foreach (var entry in inventory.GetBuildEntries())
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Bind(entry.data, entry.stack);
            activeSlots.Add(slot);
        }
    }
}
