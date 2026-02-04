using UnityEngine;

public class kangtoe99_ItemXPOrb : kangtoe99_Item
{
    [Header("XP Orb Settings")]
    [SerializeField] private int scoreValue = 10;

    protected override void OnPickup(kangtoe99_Player player)
    {
        if (kangtoe99_ScoreSystem.Instance != null)
        {
            kangtoe99_ScoreSystem.Instance.AddScore(scoreValue);
        }
        Debug.Log($"XP Orb picked up! +{scoreValue} score");
    }

    public void SetScoreValue(int value)
    {
        scoreValue = value;
    }
}
