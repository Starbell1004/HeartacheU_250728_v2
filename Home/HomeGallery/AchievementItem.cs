using UnityEngine;
using UnityEngine.UI;
using TMPro;  // TextMeshPro 네임스페이스 추가

public class AchievementItem : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI titleText;        //  Text → TextMeshProUGUI
    [SerializeField] private TextMeshProUGUI descriptionText;  //  Text → TextMeshProUGUI
    [SerializeField] private Image iconImage;
  

    [Header("숨김 설정")]
    [SerializeField] private string hiddenTitle = "???";
    [SerializeField] private string hiddenDescription = "???";
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;

    public void Setup(AchievementData data, bool isUnlocked)
    {
        // 제목
        if (titleText != null)
        {
            titleText.text = isUnlocked ? data.title : hiddenTitle;
            titleText.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = isUnlocked ? $"'{data.description}'" : hiddenDescription;
            descriptionText.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // 아이콘 (있으면)
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