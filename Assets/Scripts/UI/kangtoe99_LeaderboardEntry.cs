using UnityEngine;
using TMPro;

public class kangtoe99_LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    public void SetData(int rank, string name, int level, int score)
    {
        if (rankText != null)
        {
            rankText.text = $"#{rank}";
        }

        if (nameText != null)
        {
            nameText.text = name;
        }

        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    // 4개 텍스트(rank/name/level/score)에 일괄 적용 — 하이라이트 행 텍스트 색용.
    public void SetTextColor(Color color)
    {
        if (rankText != null) rankText.color = color;
        if (nameText != null) nameText.color = color;
        if (levelText != null) levelText.color = color;
        if (scoreText != null) scoreText.color = color;
    }

    // rank<=0 이면 순위도 알 수 없는 경우(예: 내 순위 미확보) — "-" 로 표시.
    public void SetEmpty(int rank = 0)
    {
        if (rankText != null)
        {
            rankText.text = rank > 0 ? $"#{rank}" : "-";
        }

        if (nameText != null)
        {
            nameText.text = "-";
        }

        if (levelText != null)
        {
            levelText.text = "-";
        }

        if (scoreText != null)
        {
            scoreText.text = "-";
        }
    }
}
