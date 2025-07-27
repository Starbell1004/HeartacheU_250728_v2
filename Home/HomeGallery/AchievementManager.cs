using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections;

public class AchievementManager : MonoBehaviour
{
    private static AchievementManager instance;
    public static AchievementManager Instance => instance;

    [Header("도전과제 데이터")]
    [SerializeField] private AchievementData[] allAchievements;

    [Header("팝업 UI")]
    [SerializeField] private GameObject achievementPopup;
    [SerializeField] private Text popupTitleText;
    [SerializeField] private Text popupDescriptionText;
    [SerializeField] private float popupDuration = 3f;

    private ICustomVariableManager variableManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void InitializeServices()
    {
        Engine.OnInitializationFinished -= InitializeServices;
        variableManager = Engine.GetService<ICustomVariableManager>();
    }

    // 도전과제 달성 체크 (나니노벨 스크립트에서 호출)
    public void UnlockAchievement(string achievementID)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.achievementID == achievementID)
            {
                // 이미 달성했는지 체크
                if (IsAchievementUnlocked(achievement.unlockVariable))
                    return;

                // 달성 처리
                if (variableManager != null)
                {
                    variableManager.SetVariableValue(achievement.unlockVariable, new CustomVariableValue("true"));
                }

                // 팝업 표시
                ShowAchievementPopup(achievement);
                break;
            }
        }
    }

    private bool IsAchievementUnlocked(string unlockVariable)
    {
        if (variableManager == null || string.IsNullOrEmpty(unlockVariable))
            return false;

        if (variableManager.VariableExists(unlockVariable))
        {
            var value = variableManager.GetVariableValue(unlockVariable);
            return value.ToString() == "true";
        }

        return false;
    }

    private void ShowAchievementPopup(AchievementData achievement)
    {
        if (achievementPopup == null) return;

        // 팝업 텍스트 설정
        if (popupTitleText != null)
            popupTitleText.text = "도전과제 달성!";

        if (popupDescriptionText != null)
            popupDescriptionText.text = achievement.title;

        // 팝업 표시
        StartCoroutine(ShowPopupCoroutine());
    }

    private IEnumerator ShowPopupCoroutine()
    {
        achievementPopup.SetActive(true);

        // 페이드인 효과 (선택사항)
        CanvasGroup canvasGroup = achievementPopup.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / 0.5f);
                yield return null;
            }
        }

        yield return new WaitForSeconds(popupDuration);

        // 페이드아웃 효과 (선택사항)
        if (canvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / 0.5f);
                yield return null;
            }
        }

        achievementPopup.SetActive(false);
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;
    }
}