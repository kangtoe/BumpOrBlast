using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text rankText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text scoreText;

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

    public void SetColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }

    public void SetEmpty(int rank)
    {
        if (rankText != null)
        {
            rankText.text = $"#{rank}";
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
