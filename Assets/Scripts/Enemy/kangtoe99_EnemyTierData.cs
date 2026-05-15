using UnityEngine;

// 적 등급별 수치 배율·시각 + 스폰 진행(solid → blend → solid) 데이터.
// EnemySpawner가 PickTier(경과시간)로 등급을 뽑아 Enemy.ApplyTier로 적용한다.
// 스텝별 solid/blend 시간은 각 TierEntry에서 개별 설정 (인스펙터).
// 챔피언은 등급과 별개 축 — 이 SO와 무관하게 EnemySpawner가 처리한다.
[CreateAssetMenu(fileName = "EnemyTierData", menuName = "kangtoe99/Enemy Tier Data")]
public class kangtoe99_EnemyTierData : ScriptableObject
{
    [System.Serializable]
    public class TierEntry
    {
        public kangtoe99_EnemyTier tier;
        [Tooltip("HP·Damage·Speed 공통 배율")]
        public float statMultiplier = 1f;
        [Tooltip("점수·XP 드롭 배율")]
        public float scoreMultiplier = 1f;
        public Color color = Color.gray;
        [Tooltip("스프라이트 크기 배율")]
        public float scaleMultiplier = 1f;
        [Tooltip("이 등급만 스폰되는 구간 길이(초). 마지막 등급은 무시 — 도달 후 영구 고정")]
        public float solidDuration = 30f;
        [Tooltip("이 등급 → 다음 등급 전환 구간 길이(초). 마지막 등급은 무시")]
        public float blendDuration = 20f;
    }

    [Header("등급 데이터 (Gray→Orange 순서로). 스텝별 solid/blend 시간은 각 항목에서 설정")]
    [SerializeField] private TierEntry[] tiers;

    // 등급 진행이 끝나는 시점(초) — 마지막 등급 solid가 시작되는 시각.
    // 이 시점 이후엔 등급이 더 안 오르므로, EnemySpawner는 여기서부터 시간 배율로 난이도를 잇는다.
    public float ProgressionDuration
    {
        get
        {
            if (tiers == null || tiers.Length <= 1) return 0f;
            float total = 0f;
            for (int i = 0; i < tiers.Length - 1; i++)
            {
                total += tiers[i].solidDuration + tiers[i].blendDuration;
            }
            return total;
        }
    }

    // 경과 시간으로 스폰할 등급을 뽑는다. 타임라인을 등급별 solid → blend 순서로 걷는다.
    // solid 구간이면 단일 등급, blend 구간이면 인접 두 등급을 비율(0→1)로 추첨. 마지막 등급 도달 후엔 고정.
    public TierEntry PickTier(float elapsedTime)
    {
        if (tiers == null || tiers.Length == 0) return null;

        int maxIndex = tiers.Length - 1;
        float t = Mathf.Max(0f, elapsedTime);

        for (int i = 0; i < maxIndex; i++)
        {
            if (t < tiers[i].solidDuration) return tiers[i];
            t -= tiers[i].solidDuration;

            if (tiers[i].blendDuration > 0f)
            {
                if (t < tiers[i].blendDuration)
                {
                    // blend 구간: 다음 등급으로 갈 확률이 0→1로 증가
                    return Random.value < (t / tiers[i].blendDuration) ? tiers[i + 1] : tiers[i];
                }
                t -= tiers[i].blendDuration;
            }
        }

        return tiers[maxIndex];
    }
}
