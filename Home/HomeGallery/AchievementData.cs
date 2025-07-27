using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Game/Achievement Data")]
public class AchievementData : ScriptableObject
{
    [Header("�������� ����")]
    public string achievementID = "achievement_001";
    public string title = "�������� ����";
    public string description = "�������� ����";

    [Header("�ر� ����")]
    public string unlockVariable = "G_Achievement_001";

    [Header("ǥ�� ����")]
    public Sprite icon;  // ������ (���û���)
}