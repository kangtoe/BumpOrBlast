using UnityEngine;

// 레벨업 선택지의 티어별 가중치 테이블.
// X = currentLevel(1~30, 그 이상은 30으로 클램프), Y = 상대 가중치(>= 0).
// 가중치는 절대 확률이 아니라 비율 — 합이 100일 필요는 없다.
[CreateAssetMenu(fileName = "TierDropTable_New", menuName = "BumpOrBlast/Tier Drop Table", order = 1)]
public class kangtoe99_TierDropTable : ScriptableObject
{
    [SerializeField] private AnimationCurve grayWeight;
    [SerializeField] private AnimationCurve greenWeight;
    [SerializeField] private AnimationCurve blueWeight;
    [SerializeField] private AnimationCurve purpleWeight;
    [SerializeField] private AnimationCurve orangeWeight;

    private const int MinLevel = 1;
    private const int MaxLevel = 30;

    public float Evaluate(kangtoe99_Tier tier, int level)
    {
        float x = Mathf.Clamp(level, MinLevel, MaxLevel);
        AnimationCurve c = GetCurve(tier);
        if (c == null || c.length == 0) return 0f;
        return Mathf.Max(0f, c.Evaluate(x));
    }

    private AnimationCurve GetCurve(kangtoe99_Tier tier)
    {
        switch (tier)
        {
            case kangtoe99_Tier.Gray:   return grayWeight;
            case kangtoe99_Tier.Green:  return greenWeight;
            case kangtoe99_Tier.Blue:   return blueWeight;
            case kangtoe99_Tier.Purple: return purpleWeight;
            case kangtoe99_Tier.Orange: return orangeWeight;
            default: return null;
        }
    }

    // 신규 자산 생성 시 및 인스펙터 Reset 메뉴에서 호출 — 기본 곡선 채움.
    // 키프레임: level 1 / 10 / 20 / 30.
    private void Reset()
    {
        grayWeight   = MakeCurve(70, 30, 10, 5);
        greenWeight  = MakeCurve(25, 35, 25, 15);
        blueWeight   = MakeCurve(5,  25, 30, 25);
        purpleWeight = MakeCurve(0,  8,  25, 35);
        orangeWeight = MakeCurve(0,  2,  10, 20);
    }

    private static AnimationCurve MakeCurve(float v1, float v10, float v20, float v30)
    {
        return new AnimationCurve(
            new Keyframe(1f,  v1),
            new Keyframe(10f, v10),
            new Keyframe(20f, v20),
            new Keyframe(30f, v30)
        );
    }
}
