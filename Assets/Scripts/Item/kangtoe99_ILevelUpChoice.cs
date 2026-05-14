using UnityEngine;

// LevelUpSystem의 선택지 풀에 들어갈 수 있는 데이터.
// ItemData(영구 modifier) / InstantScoreItemData(즉시 효과) 등이 구현.
public interface kangtoe99_ILevelUpChoice
{
    string DisplayName { get; }
    string Description { get; }
    Sprite Icon { get; }
    bool IsAvailable(GameObject player);
    void Apply(GameObject player);
}
