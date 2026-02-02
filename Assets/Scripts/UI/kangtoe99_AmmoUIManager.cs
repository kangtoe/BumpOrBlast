using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kangtoe99_AmmoUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject ammoIconPrefab;
    [SerializeField] private RectTransform ammoIconContainer;

    [Header("Casing Physics Settings")]
    [SerializeField] private float casingUpwardForce = 300f;
    [SerializeField] private float casingRandomHorizontal = 50f;
    [SerializeField] private float casingLifetime = 1.5f;
    [SerializeField] private float casingRandomTorque = 500f;
    [SerializeField] private float casingGravityScale = 5f;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color reloadingColor = Color.red;

    private List<kangtoe99_AmmoIcon> ammoIcons = new List<kangtoe99_AmmoIcon>();
    private Coroutine reloadCoroutine;

    /// <summary>
    /// 탄창 UI 생성
    /// </summary>
    public void InitializeAmmoUI(int currentAmmo, int maxAmmo)
    {
        // 진행 중인 재장전 코루틴 중지
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        // ammoIconContainer의 모든 자식 제거 (미리보기용 배치 포함)
        foreach (Transform child in ammoIconContainer)
        {
            Destroy(child.gameObject);
        }
        ammoIcons.Clear();

        // 새로운 아이콘 생성
        for (int i = 0; i < maxAmmo; i++)
        {
            GameObject iconObj = Instantiate(ammoIconPrefab, ammoIconContainer);
            kangtoe99_AmmoIcon icon = iconObj.GetComponent<kangtoe99_AmmoIcon>();
            if (icon != null)
            {
                icon.SetActive(true);
                icon.SetFillAmount(1f);
                icon.SetFillColor(normalColor);

                // currentAmmo 이상의 인덱스는 Fill 비활성화 (사용된 탄환)
                if (i >= currentAmmo)
                {
                    GameObject fillObject = icon.GetFillObject();
                    if (fillObject != null)
                    {
                        fillObject.SetActive(false);
                    }
                }

                ammoIcons.Add(icon);
            }
        }
    }

    /// <summary>
    /// 발사 시 호출: Fill 이미지 비활성화
    /// </summary>
    public void OnShoot(Vector3 shootPosition)
    {
        // 마지막 활성 아이콘 찾기
        kangtoe99_AmmoIcon lastIcon = FindLastActiveIcon();
        if (lastIcon == null) return;

        // Fill 이미지 비활성화 (배경은 유지)
        GameObject fillObject = lastIcon.GetFillObject();
        if (fillObject == null) return;

        fillObject.SetActive(false);
    }

    /// <summary>
    /// 재장전 시작 시 호출
    /// </summary>
    public void OnReloadStart(float duration)
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }
        reloadCoroutine = StartCoroutine(AnimateReloadSequential(duration));
    }

    /// <summary>
    /// 순차 재장전 애니메이션 코루틴
    /// </summary>
    private IEnumerator AnimateReloadSequential(float duration)
    {
        int iconCount = ammoIcons.Count;
        if (iconCount == 0)
        {
            yield break;
        }

        // 모든 아이콘 활성화 + 빨간색 + fillAmount = 0
        foreach (var icon in ammoIcons)
        {
            icon.SetActive(true);
            icon.SetFillColor(reloadingColor);  // 빨간색
            icon.SetFillAmount(0f);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration); // 0~1

            // 전체 진행률에 따라 몇 개의 탄창을 채울지 계산
            float totalProgress = progress * iconCount; // 0 ~ iconCount
            int completedIcons = Mathf.FloorToInt(totalProgress); // 완전히 채워진 탄창 개수
            float currentIconProgress = totalProgress - completedIcons; // 현재 채우는 탄창의 진행률

            // 완전히 채워진 탄창들
            for (int i = 0; i < completedIcons && i < iconCount; i++)
            {
                ammoIcons[i].SetFillAmount(1f);
            }

            // 현재 채우는 중인 탄창
            if (completedIcons < iconCount)
            {
                ammoIcons[completedIcons].SetFillAmount(currentIconProgress);
            }

            // 아직 안 채운 탄창들
            for (int i = completedIcons + 1; i < iconCount; i++)
            {
                ammoIcons[i].SetFillAmount(0f);
            }

            yield return null;
        }

        // 완료: 모두 하얀색으로 변경
        foreach (var icon in ammoIcons)
        {
            icon.SetFillAmount(1f);
            icon.SetFillColor(normalColor);  // 하얀색
        }

        reloadCoroutine = null;
    }

    /// <summary>
    /// 마지막 활성 아이콘 찾기
    /// </summary>
    private kangtoe99_AmmoIcon FindLastActiveIcon()
    {
        for (int i = ammoIcons.Count - 1; i >= 0; i--)
        {
            if (ammoIcons[i] != null && ammoIcons[i].IsActive())
            {
                return ammoIcons[i];
            }
        }
        return null;
    }
}
