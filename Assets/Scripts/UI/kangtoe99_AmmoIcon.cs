using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_AmmoIcon : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;  // 검은색 배경
    [SerializeField] private Image fillImage;        // 하얀색/빨간색 내용물

    /// <summary>
    /// 아이콘의 활성/비활성 상태 설정
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (fillImage != null)
        {
            fillImage.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 내용물의 채움 정도 설정 (0~1)
    /// </summary>
    public void SetFillAmount(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
        }
    }

    /// <summary>
    /// 내용물의 색상 설정 (하얀색/빨간색)
    /// </summary>
    public void SetFillColor(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }

    /// <summary>
    /// 아이콘이 활성화되어 있는지 확인 (Fill 이미지 기준)
    /// </summary>
    public bool IsActive()
    {
        return fillImage != null && fillImage.gameObject.activeSelf;
    }

    /// <summary>
    /// Fill 이미지 GameObject 가져오기 (물리 효과용)
    /// </summary>
    public GameObject GetFillObject()
    {
        return fillImage != null ? fillImage.gameObject : null;
    }
}
