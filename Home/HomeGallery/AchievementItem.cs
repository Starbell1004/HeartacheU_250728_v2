using UnityEngine;
using UnityEngine.UI;
using TMPro;  // TextMeshPro ���ӽ����̽� �߰�

public class AchievementItem : MonoBehaviour
{
    [Header("UI ���")]
    [SerializeField] private TextMeshProUGUI titleText;        //  Text �� TextMeshProUGUI
    [SerializeField] private TextMeshProUGUI descriptionText;  //  Text �� TextMeshProUGUI
    [SerializeField] private Image iconImage;
  

    [Header("���� ����")]
    [SerializeField] private string hiddenTitle = "???";
    [SerializeField] private string hiddenDescription = "???";
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;

    public void Setup(AchievementData data, bool isUnlocked)
    {
        // ����
        if (titleText != null)
        {
            titleText.text = isUnlocked ? data.title : hiddenTitle;
            titleText.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // ����
        if (descriptionText != null)
        {
            descriptionText.text = isUnlocked ? $"'{data.description}'" : hiddenDescription;
            descriptionText.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // ������ (������)
        if (iconImage != null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = isUnlocked ? unlockedColor : lockedColor;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

       
    }
}