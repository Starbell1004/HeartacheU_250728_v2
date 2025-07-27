using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Game/Achievement Data")]
public class AchievementData : ScriptableObject
{
    [Header("도전과제 정보")]
    public string achievementID = "achievement_001";
    public string title = "도전과제 제목";
    public string description = "도전과제 설명";

    [Header("해금 조건")]
    public string unlockVariable = "G_Achievement_001";

    [Header("표시 설정")]
    public Sprite icon;  // 아이콘 (선택사항)
}