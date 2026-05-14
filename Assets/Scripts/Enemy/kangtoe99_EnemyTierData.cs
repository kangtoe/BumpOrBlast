using UnityEngine;

// 적 등급별 수치 배율·시각 + 스폰 진행(solid → blend → solid) 데이터.
// EnemySpawner가 PickTier(경과시간)로 등급을 뽑아 Enemy.ApplyTier로 적용한다.
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
    }

    [Header("등급 데이터 (Gray→Orange 순서로 채울 것)")]
    [SerializeField] private TierEntry[] tiers;

    [Header("스폰 진행 (solid → blend → solid ...)")]
    [Tooltip("한 등급만 스폰되는 구간 길이(초)")]
    [SerializeField] private float solidDuration = 30f;
    [Tooltip("인접 두 등급이 섞이는 전환 구간 길이(초)")]
    [SerializeField] private float blendDuration = 20f;

    // 경과 시간으로 스폰할 등급을 뽑는다.
    // cycle = solid + blend. solid 구간이면 단일 등급, blend 구간이면 인접 두 등급을 비율(0→1)로 추첨.
    // 최고 등급 도달 후엔 그 등급으로 고정.
    public TierEntry PickTier(float elapsedTime)
    {
        if (tiers == null || tiers.Length == 0) return null;

        int maxIndex = tiers.Length - 1;
        float cycle = Mathf.Max(0.01f, solidDuration + blendDuration);
        int index = Mathf.FloorToInt(Mathf.Max(0f, elapsedTime) / cycle);
        if (index >= maxIndex) return tiers[maxIndex];

        float within = elapsedTime - index * cycle;
        if (within < solidDuration) return tiers[index];

        // blend 구간: 다음 등급으로 갈 확률이 0→1로 증가
        float t = (within - solidDuration) / Mathf.Max(0.01f, blendDuration);
        return Random.value < t ? tiers[index + 1] : tiers[index];
    }
}
